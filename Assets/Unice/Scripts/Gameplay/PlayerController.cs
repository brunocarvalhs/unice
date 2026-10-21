using System;

namespace Unice
{
    /// <summary>
    /// This device's player once the input is read: moving the character, the noise it makes, and the hide/pass/laugh/open
    /// actions. The host applies actions directly; a guest sends them to the host in its presence (see
    /// <see cref="actionSeq"/>). Keys, joystick and sounds belong to the layer above, which feeds this class and listens through <see cref="IStage"/>.
    /// </summary>
    public sealed class PlayerController
    {
        /// <summary>Joystick/keys below this length count as no input.</summary>
        public const float InputDeadZone = .3f;
        /// <summary>After stepping into a spot, input is ignored this long so the character doesn't walk straight out.</summary>
        public const float HideFreezeSeconds = .35f;
        /// <summary>Guest Unice: seconds between two catch requests, so one touch doesn't send a burst.</summary>
        public const float CatchRequestInterval = .3f;
        /// <summary>How fast the HUD's noise meter follows how loud the player is (per second).</summary>
        const float NoiseFollowRate = 6;
        /// <summary>Movement below this many house pixels in a step counts as standing still.</summary>
        const float MinMovement = .01f;

        readonly Game game;
        readonly Func<Character> myCharacter;
        readonly IStage stage;
        readonly Func<bool> isPaused;
        readonly Action publishSoon;
        float freezeTime, catchCooldown;

        /// <summary>The last action, published in this device's presence; the host applies it once per new <see cref="actionSeq"/>.</summary>
        public int actionSeq, actionArg;
        public PlayerAction action;

        Character me => myCharacter();

        /// <param name="isPaused">The pause menu is open (solo: nothing moves).</param>
        /// <param name="publishSoon">A guest asks for its presence to be sent now, because it has a new action.</param>
        public PlayerController(Game game, Func<Character> myCharacter, IStage stage, Func<bool> isPaused, Action publishSoon)
        {
            this.game = game; this.myCharacter = myCharacter; this.stage = stage; this.isPaused = isPaused; this.publishSoon = publishSoon;
        }

        /// <summary>New room or match: no pending action.</summary>
        public void Reset() { actionSeq = 0; action = PlayerAction.None; actionArg = -1; }

        public void TickCooldowns(float dt)
        {
            if (freezeTime > 0) freezeTime -= dt;
            if (catchCooldown > 0) catchCooldown -= dt;
        }

        /// <summary>Keys plus joystick as a unit direction; (0, 0) inside the dead zone.</summary>
        public static (float x, float y) Direction(float keysX, float keysY, float joystickX, float joystickY)
        {
            float x = keysX + joystickX, y = keysY + joystickY;
            float length = (float)Math.Sqrt(x * x + y * y);
            return length < InputDeadZone ? (0, 0) : (x / length, y / length);
        }

        /// <summary>
        /// Moves this player along <paramref name="direction"/> (a unit vector or zero). As the Unice on a guest device, also asks the
        /// host to catch whoever it touches.
        /// </summary>
        public void Move(float dt, (float x, float y) direction, bool sneakInput)
        {
            var character = me; character.moving = false;
            if (!game.CanMove(character) || freezeTime > 0 || isPaused()) { character.loud = false; return; }

            character.sneaking = !character.IsUnice && sneakInput;
            if (direction.x != 0 || direction.y != 0)
            {
                float speed = character.IsUnice ? Rules.UnicePlayerSpeed : character.sneaking ? Rules.SneakSpeed : Rules.RunSpeed;
                float oldX = character.x, oldY = character.y;
                Game.MoveCharacter(character, direction.x * speed * dt, direction.y * speed * dt);
                character.faceX = direction.x; character.faceY = direction.y;
                character.moving = Geometry.Distance(character.x, character.y, oldX, oldY) > MinMovement;
            }
            character.loud = !character.IsUnice && character.moving && !character.sneaking;

            // the host catches by touch itself (Game.Simulate); a guest Unice asks for it
            if (character.IsUnice && game.phase == Phase.Hunt && !game.isHost && catchCooldown <= 0)
                foreach (var hider in game.characters)
                    if (!hider.IsUnice && !hider.outOfRound && hider.hidingSpot == null && Geometry.Distance(character, hider) < Rules.CatchRange)
                    {
                        SendAction(PlayerAction.Catch, hider.index);
                        catchCooldown = CatchRequestInterval;
                        break;
                    }
        }

        /// <summary>Per step during a round: the HUD's noise meter follows how loud the player is, smoothly.</summary>
        public void UpdateNoiseLevel(float dt)
        {
            var character = me;
            if (character == null) return;
            float target = character.loud ? 1 : 0;
            character.noiseLevel += (target - character.noiseLevel) * Math.Min(1, dt * NoiseFollowRate);
        }

        void SendAction(PlayerAction newAction, int arg = -1)
        {
            if (game.isHost) { game.ApplyAction(me, newAction, arg); return; }
            actionSeq++; action = newAction; actionArg = arg;
            publishSoon();
        }

        /// <summary>E: hide in / leave the spot nearby, or (Unice) open it.</summary>
        public void UseNearbySpot()
        {
            var character = me;
            if (character == null || character.outOfRound || !game.IsLive || isPaused()) return;
            if (character.IsUnice)
            {
                if (game.phase != Phase.Hunt || character.stunTime > 0) return;
                var spotToOpen = Game.NearestSpot(character);
                if (spotToOpen == null) { stage.Toast(Localization.Get("act.unice.need_near")); return; }
                SendAction(PlayerAction.Open, spotToOpen.spotIndex);
                stage.Log("player_action", ("type", "open"));
                return;
            }
            if (character.hidingSpot != null)
            {
                stage.Sound("creak"); SendAction(PlayerAction.Exit);
                stage.Log("player_action", ("type", "exit"));
                return;
            }
            var spot = Game.NearestSpot(character);
            if (spot == null) { stage.Toast(Localization.Get("act.hide.need_near")); return; }
            if (spot.occupant != null) { stage.Toast(Localization.Get("act.hide.occupied", spot.occupant.name)); stage.Sound("bad"); return; }
            stage.Sound("creak"); freezeTime = HideFreezeSeconds;
            SendAction(PlayerAction.Hide, spot.spotIndex);
            stage.Log("player_action", ("type", "hide"), ("spot", spot.type));
        }

        /// <summary>Q: hand the doll to someone right next to you.</summary>
        public void TryPassDoll()
        {
            var character = me;
            if (character == null || character.outOfRound || character.IsUnice || !game.IsLive || isPaused()) return;
            if (game.doll.holder != character) { stage.Toast(Localization.Get("act.pass.no_doll")); return; }
            if (character.passCooldown > 0) { stage.Toast(Localization.Get("act.pass.cooldown", (int)Math.Ceiling(character.passCooldown))); stage.Sound("bad"); return; }
            if (game.PassTarget(character) == null) { stage.Toast(Localization.Get("act.pass.no_target")); return; }
            SendAction(PlayerAction.Pass);
            stage.Log("player_action", ("type", "pass"));
        }

        /// <summary>R: laugh on purpose. Anyone can, but it gives away your position to the Unice like the doll's own giggle.</summary>
        public void TryLaugh()
        {
            var character = me;
            if (character == null || character.outOfRound || character.IsUnice || !game.IsLive || isPaused()) return;
            if (character.laughCooldown > 0) { stage.Toast(Localization.Get("act.laugh.cooldown", (int)Math.Ceiling(character.laughCooldown))); stage.Sound("bad"); return; }
            SendAction(PlayerAction.Laugh);
            stage.Log("player_action", ("type", "laugh"));
        }
    }
}
