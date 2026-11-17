# Dream Football 2026 — Zakareya (Unity Native)

This folder begins a **native Unity Android game**, separate from the legacy browser prototype in the repository root.

## Current status

This is a project foundation, **not a playable football game yet**. It deliberately contains no capsule-player stand-ins, fake screenshots, or unlicensed DLS assets. A real visual prototype requires licensed realistic humanoid models, a rig, animation clips, pitch materials, stadium assets, and a tested Android build.

## Editor baseline
- Unity Editor: 6000.0.32f1
- Render pipeline: Universal Render Pipeline (URP), mobile-oriented
- Input: Unity Input System
- Target: Android ARM64
- Intended offline mode: all match assets and gameplay code packaged locally; no runtime network dependency.

Open this folder (UnityProject) as the Unity project root.

## Implemented code foundation (not yet Unity-tested)

- `MatchClock` and `MatchController`: separate 45-minute halves, pause/resume, automatic halftime/fulltime transitions, and inspector-time validation of clock settings.
- `PlayerTeamMember` and `BallPossessionTracker`: team identity metadata and a lightweight proximity/ball-speed estimate of the current player and team in possession, with possession-change events and manual release.
- `TeamTacticalController`: connects the possession estimate to basic team behavior. Only the current AI owner is allowed to auto-pass; the nearest configured opponent is designated to press and attempt automatic tackles. It can hand movement control from `TeamPositioningAI` to `TeamPressingAI` for the designated presser.
- `FootballerMotor`: character-controller locomotion with Animator parameters (`MoveSpeed`, `IsSprinting`) and a read-only movement-state interface.
- `TeamPositioningAI`: lightweight offline tactical positioning for individual AI outfield players using formation home anchors, role-based ball influence, pitch bounds, and smoothed movement. Its role and attack direction can be set by a formation manager.
- `TeamFormationController`: creates formation anchors for ten outfield players and supports 4-3-3, 4-4-2, and 3-5-2 presets, with mirrored X positions for the opposite attack direction. The goalkeeper is assigned separately.
- `TeamPressingAI`: optional defensive pressure behavior for a designated AI player, with engage/release distance hysteresis, a configurable stand-off distance, and a return-to-anchor behavior. It requires an external match/team state to enable pressing and is not a full team press or possession system.
- `TeamPassingAI`: selects an assigned teammate using distance, forward progress, receiver space, and opponent lane-risk scoring, then applies a pass impulse when enabled and the ball is within control range. It is an early decision prototype, not a full possession or pass-animation system.
- `TackleAction`: close-range tackle attempt for human or AI defenders. It checks range, ball height, and facing angle before deflecting the Rigidbody ball; it supports manual touch input or optional automatic attempts.
- `BallControlAssist`: capped Rigidbody force assistance that keeps a nearby ball in front of a moving player for a basic dribbling prototype.
- `TouchJoystick`: UI pointer bridge for a virtual movement stick.
- `TouchActionButton`: touch action bridge for sprint (hold), pass, shot and optional tackle actions.
- `MatchBall` and `PlayerBallActions`: Rigidbody ball physics plus range-checked pass/shot impulses; ball tuning values are clamped during Inspector validation.
- `PitchBuilder`: provisional regulation-sized pitch surface and markings for blockout tests. This is geometry for testing gameplay, not final stadium art.
- `MatchFollowCamera`: smoothed third-person camera that follows the controlled player and subtly accounts for the ball.
- `GoalkeeperMotor`: basic lateral positioning with a short linear estimate of the ball's goal-line crossing point when it is approaching; this is not a save, dive, or catch system.
- `GoalkeeperSaveZone`: optional trigger volume that deflects an incoming ball away from the defended goal with configurable rebound speed, lateral direction and lift; this is a physics prototype, not an animated save.
- `MatchScoreboard` and `GoalTrigger`: score tracking and goal-mouth trigger registration for the ball; goal volumes enforce trigger-collider setup and warn when the scoreboard reference is missing.
- `KickoffResetter`: optional delayed restart helper that restores the ball and assigned players to explicit spawn transforms.

These scripts still need to be assembled in a Unity scene, assigned references/materials, compiled in the target Unity Editor, and tested on Android. No playable APK exists yet.

## Planned implementation sequence
1. **Project validation:** verify Unity package compatibility, complete project settings, URP renderer, input and Android build configuration.
2. **Visual vertical slice:** licensed realistic humanoid player, humanoid rig, retargeted locomotion/turn/sprint/kick animations, pitch and stadium lighting.
3. **Playable match:** integrate and test ball control, passing, shooting, goal detection/restart, goalkeeper saves, touch controls and camera in a real scene.
4. **Team simulation:** extend `TeamPositioningAI` into coordinated 11v11 formations, player roles, marking, pressing, passing decisions, opponent AI, substitutions and match rules.
5. **Offline Android build:** package all required assets, build ARM64 APK in CI, install and profile on the target phone.

## Important build constraint

GitHub Actions cannot activate Unity by guessing credentials. A cloud build needs a Unity Personal/Pro license activation configured as GitHub Actions secrets, or a compatible Unity build service. Never add license files or credentials to the repository.

## Asset and performance policy
- Use original or explicitly licensed models, animations, audio, kits, badges and fonts.
- Do not extract DLS assets or use official club/player likenesses without permission.
- Performance targets must be validated on the actual device; specifications alone do not establish real frame rate.
- Aim for scalable quality settings and a stable 30 FPS baseline before attempting 60 FPS.

## Legacy browser version

The web prototype remains in the repository root and is not the native Unity project. It will not be presented as the requested finished game.

## Scene wiring checklist (manual Unity Editor work still required)

1. Create a match scene and add a ground object with a collider. Add `PitchBuilder` to an empty parent; assign grass and line materials where available.
2. Add a player GameObject with `CharacterController`, `FootballerMotor`, `PlayerBallActions`, and `BallControlAssist`. Assign the follow camera to the motor's movement-camera field and assign the ball to both ball-related components.
3. Create a Canvas with a `GraphicRaycaster` and an `EventSystem` using the Input System UI module. Add a joystick background and handle; assign both RectTransforms and the player motor to `TouchJoystick`.
4. Add separate UI touch targets with `TouchActionButton`: configure one as Sprint and assign the motor; configure Pass and Shoot buttons and assign the player's `PlayerBallActions`; optionally configure a Tackle button and assign the player's `TackleAction`. Optionally assign world-space pass/shot target transforms. If no target is assigned, the action uses the controlled player's forward direction.
5. Add a Rigidbody and collider to the ball and attach `MatchBall`. Add a `MatchScoreboard` to a match manager. Each goal mouth needs a trigger collider and `GoalTrigger` with its team-side setting and scoreboard reference.
6. Add `MatchController` and `MatchClock` to the match manager. For restarts, configure `KickoffResetter` with the ball, a ball kickoff transform, and matching player/player-spawn arrays.
7. Add `BallPossessionTracker` to the match manager, assign the shared ball and every player (each with `PlayerTeamMember`), and set each player's team ID consistently (for example, 0 for home and 1 for away). Tune claim/release distances and ball-speed thresholds in play tests.
8. Add `TeamFormationController` to the match manager at the pitch center. Set the team's ID and attack direction, choose a formation preset, and assign the ten outfield players with `PlayerTeamMember`, `TeamPositioningAI`, and slot indexes 0–9. Slot indexes follow the preset's listed order in the script (back line first, then midfield, then forwards); the goalkeeper is not included. Assign the shared ball to each `TeamPositioningAI`. Do not combine `TeamPositioningAI` with another component that moves the same player Transform.
9. Add `TeamTacticalController` to the match manager. Assign the `BallPossessionTracker`, the same `MatchBall`, and a PlayerUnit entry for each AI player with its `PlayerTeamMember`, optional `TeamPositioningAI`, and any applicable `TeamPassingAI`, `TeamPressingAI`, and `TackleAction` references. When both positioning and pressing are assigned, the controller pauses formation movement for the designated presser and hands that player's Transform to `TeamPressingAI`; formation movement resumes for that player when it is no longer the designated presser. Add at least the nearest defender candidates with a pressing or tackling component; the controller selects the closest eligible opponent. It enables auto-passing only for the current AI owner and designates one nearest defender for pressing/automatic tackling. Do not assign movement components that fight over the same Transform.
10. Add `MatchFollowCamera` to the camera and assign its target player. Add `GoalkeeperMotor` to each goalkeeper and assign the ball's `MatchBall` component. Set `defendPositiveXGoal` according to the end being defended. Optionally add `GoalkeeperSaveZone` to a trigger collider in the keeper's reachable area; set its defended end to match the goalkeeper. The ball's Rigidbody must be able to enter this trigger.

The checklist describes required manual setup; it does not mean a scene has already been created or validated. UI touch controls require a functioning EventSystem, and all serialized references must be assigned in the Inspector.

## Latest hardening

Clock timing values and ball physics tuning values are clamped to safe minimums during Inspector validation. Goal triggers automatically mark their collider as a trigger and warn if no scoreboard is assigned. These checks reduce common setup mistakes but are not a substitute for compiling and testing the scene in the target Unity Editor.

## Ball control notes

`BallControlAssist` is an optional early dribbling aid, not a full possession system. It only applies a capped planar Rigidbody force while the controlled player is moving and the ball is close and near ground level. Tune its control radius, desired distance, gain, damping, and acceleration in the Inspector after testing with the actual ball mass, collider, and player scale. Disable the component if it causes unwanted ball pull or jitter. It does not provide foot-contact animation, shielding, tackles, or persistent team possession.

## Goalkeeper positioning notes

`GoalkeeperMotor` estimates the ball's lateral crossing point at the configured goal line using its current velocity, but only when the ball is moving toward the defended end and the estimated crossing is within the prediction horizon. This is a simple linear estimate: it does not model ball bounce, spin, drag during the prediction interval, keeper reach, or save animations. Tune `predictionHorizon`, `minimumApproachSpeed`, `reactionDistance`, and `lateralSpeed` after the component is connected to a real match scene. It remains positioning logic only; diving, catching, parrying, and save decisions are not implemented.


## Goalkeeper save-zone notes

Place `GoalkeeperSaveZone` on a trigger collider covering the intended keeper contact area. Configure the defended X end consistently with `GoalkeeperMotor`. When the ball enters while travelling toward that goal above the minimum incoming speed, the component redirects its Rigidbody velocity away from the goal and adds a configurable lateral rebound and small lift. The trigger is intentionally optional and should not overlap the goal line or field in a way that causes unintended deflections. The response is immediate and has no dive animation, reach check, hand contact, catch, or keeper decision AI. Test trigger dimensions and rebound tuning in Unity before treating it as match-ready.


## Team positioning AI notes

Add `TeamPositioningAI` to each AI-controlled outfield player, assign the match ball, and place a separate home-anchor Transform at that player's intended formation position. Select Defender, Midfielder, or Forward and set the team's attack direction consistently. The component uses the anchor as its shape reference, shifts toward the ball with role-dependent influence, clamps targets to the configured pitch bounds, and smoothly moves and faces the ball. If no anchor is assigned, the player's position at startup is used as its home position. Its movement can be suspended with `SetPositioningEnabled` so another movement component can temporarily take over. This is a lightweight positioning layer only: it does not yet implement team-wide spacing coordination, offside logic, marking assignments, possession decisions, passes, tackles, or goalkeeper behavior. All values require tuning in a real Unity scene.


## Tackle action prototype notes

Add `TackleAction` to a defender and assign the shared `MatchBall`. For manual control, create a UI touch target using `TouchActionButton`, select its Tackle action, and assign the defender's `TackleAction` reference. For a simple AI defender, call `SetAutomaticTackling(true)` while defending and disable it when the team is no longer defending. A tackle succeeds only when the ball is close, within the configured height, and inside the allowed facing arc; it then applies a configurable Rigidbody impulse away from the player's feet/front. Tune range, facing threshold, cooldown, and impulse with the actual player scale and ball mass. This prototype deflects the ball; it does not transfer possession, choose a tackle animation, perform a sliding tackle, assess foul risk, or guarantee a clean recovery. The component should not be treated as a complete interception/possession system until it is integrated with team-state logic and play-tested.

## Defensive pressing prototype notes

Add `TeamPressingAI` to one designated outfield AI player, assign the ball and a home-anchor Transform, then call `SetPressingEnabled(true)` when the team enters a defending phase and `SetPressingEnabled(false)` when it stops defending. The component engages within `engageDistance`, releases at `releaseDistance` to reduce rapid state toggling, approaches to the configured stand-off distance, and returns toward its home anchor when inactive. Do not attach it to a player whose Transform is simultaneously moved by `TeamPositioningAI`, `FootballerMotor`, or another movement script. When used alone, it returns toward its home anchor while disabled. `TeamTacticalController` can switch it to externally managed mode so it stops writing to the Transform while disabled; this permits `TeamPositioningAI` to resume without two movement scripts competing. The current prototype does not coordinate cover/shadow players or implement full interception logic. Test the movement and distances in a Unity scene before use.


## AI passing prototype notes

Add `TeamPassingAI` to an AI player and assign the shared `MatchBall`, that player's teammate Transform array (excluding itself), and the opposing-player Transform array. Set the attack direction consistently for the team. When the ball enters `controlRange`, the script scores eligible receivers using pass distance, forward progress, nearby opponent space, and the closest opponent's exposure along the pass lane; it then kicks toward the highest-scoring receiver. Tune the pass impulse and selection weights in a real scene. The script does not establish actual possession, verify that the AI player has won the ball, animate a kick, predict moving receivers, or guarantee that a pass cannot be intercepted. Do not run this component on the human-controlled player unless autonomous passing is intended. It has not yet been compiled or play-tested in Unity.


## Ball possession tracking notes

Add `PlayerTeamMember` to every player and assign a consistent integer team ID. Add one `BallPossessionTracker` to the match manager, assign the shared ball and the complete player list, and tune the claim/release distances and speed thresholds. The tracker periodically estimates the nearest eligible player as the owner only when the ball is moving slowly; it releases ownership when the ball travels away or accelerates beyond the release threshold. Read `CurrentOwner`/`CurrentTeamId` or subscribe to `PossessionChanged` to drive higher-level team states. This is a proximity heuristic, not confirmed foot control: it does not arbitrate simultaneous tackles, distinguish intentional touches, or guarantee that the player visually controls the ball. Pass/shot/tackle systems are not yet wired to force an immediate release, so tune thresholds and integrate explicit release calls as the match controller evolves.


## Team tactical controller notes

`TeamTacticalController` is the first orchestration layer connecting the possession estimate to team behavior. Add it beside `BallPossessionTracker` on the match manager and assign the same ball/tracker references. Create a player entry for each AI player, linking its team identity and optional passing, pressing, and tackle components. During possession, only the player currently reported as owner has its `TeamPassingAI` enabled. The controller selects the nearest eligible player from the opposing team as the single designated presser and automatic tackler. Add the player's `TeamPositioningAI` reference in the PlayerUnit when that player also uses formation positioning; the controller disables that positioning motor while the presser is active, then restores it when pressing is no longer assigned. This prevents two components from writing to the same Transform. When possession becomes temporarily unknown, the controller retains the last possessing team for `looseBallGracePeriod`; passing remains disabled until a current owner is identified. This is a limited prototype: it does not coordinate a full defensive block, choose formations, evaluate foul risk, manage human-controlled players, or confirm possession beyond the tracker heuristic. Validate component references and behavior in Unity before use.


## Team formation controller notes

Place one `TeamFormationController` at the pitch center for each team. Configure its `teamId`, `attackPositiveX`, and formation preset, then assign the ten outfield players. Each assignment contains the matching `PlayerTeamMember`, that player's `TeamPositioningAI`, and a slot index from 0 to 9. Slot order is defined in `GetSlots`: four defenders then three midfielders then three forwards for 4-3-3; four defenders, four midfielders, then two forwards for 4-4-2; or three defenders, five midfielders, then two forwards for 3-5-2. The controller creates child anchor objects at runtime and assigns each player's role, anchor, and attack direction. The goalkeeper is deliberately excluded and continues to use its own goalkeeper logic. Set the pitch dimensions to match the scene and keep the controller origin centered on the field. `TeamPositioningAI` owns movement of its player's Transform; do not run it with another motor or positioning component that also moves that Transform. These presets are initial blockout shapes, not fully validated professional tactical models; compile and tune them in Unity before gameplay use.
