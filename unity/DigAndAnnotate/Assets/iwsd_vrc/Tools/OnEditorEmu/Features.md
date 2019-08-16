# Features

## Implemented

### Local player

- WASD move
- mouse to view direction control
- mouse click to Interact to a object
- Pickup operation
    - pickup, drop
    - use down
- world descriptor setting (not completed)
    - player spawn position
- Quick menu
    - (as world space canvas "diegetic UI")
    - Toggle quick menu: TAB key
    - Operations
        - Respawn
        - Quit emulator program
- mouse cursor capture
    - Press ESC key to exit from emulator
    - Click Game window to go back emulator

### Trigger-action system

- Custom trigger
    - from Unity Animation event
    - ActivateCustomTrigger action
    - from uGUI component.
        - Button and TextField only
        - Not supported when UI components are in newly spawned object.
- Rondomized selection of actions with specified probabilitiy
- Almost all actions (in basic use)
    - Animation actions (AnimationInt etc.)
    - Physics actions (AddVelocity etc.)
    - Unity component dependent actions
        - PlayAnimation
        - SetParticlePlaying
        - AudioTrigger
        - SetUIText
    - VRC component dependent actions
        - SendRPC
        - (RIP WebPanel)
    - Resource access actions
        - SetMaterial
        - Spawn
- Pickup, UseDown, UseUp, Drop triggers
- Timer
    - Compatibility is not checked because of defects of original implantation
    - https://vrchat.canny.io/bug-reports/p/ontimer-doesnt-work-properly
- Physics Collision
    - Both trigger and collider
    - OnAvatarHit
    - OnParticleCollision
    - Slightly different than buggy original. Compatibility check is not completed.
        - https://vrchat.canny.io/bug-reports/p/without-onentertrigger-onexittrigger-always-fires-individualy
        - https://vrchat.canny.io/bug-reports/p/onexittrigger-wrongly-fired-if-different-layers-are-specified-in-onentertrigger-
- Life cycle (spawn, enable, disable, destroy)
    - Compatibility test for OnDestroy is not completed because of defects of original implantation
        - https://vrchat.canny.io/bug-reports/p/ondestroy-trigger-does-not-work-before-second-player-joins-the-world-instance
- Key

## Not implemented

- Network related
    - Multi user
    - Trigger system broadcast type
    - SendRPC target
    - Voice chat features
- VRC components other than VRC_Trigger
    - (Some will work without tewaks. VRC_AudioBank seems working via RPC)
- Avatar related features
- Limitation by VRChat client
    - Audio mixer
    - Light (? not investigate yet.)
        - (Some people say lighting is something different between in-Unity-editor and in-VRChat-client.)


### Local player

- support VRC_SceneDescriptor setting
    - respawn height
    - Reference camera
- support VRC_PlayerMods
    - jump power
    - speed
- Draw outline shape for interactable and pickupable objects
- Interact raycast length
- player object collider
- player object layer


### Trigger-action system

- Triggers
    - Relay
    - VRC component dependent trigger (OnVideoStart etc.)
- Delay before action
- Extra (minor) featurs of action
    - TeleportPlayer AlignRoomToDestination switch
    - SendRPC (extra featurs. player ID feature etc.)
    - SetMaterial, SpawnObject asset accessing bug reproduce and report
- VRC component dependent actions
    - Combat system
- Pickup minor features
    - indirect holding (move holding object with physics but not kinematic)
    - change hover text
    - AutoHold AutoDetect
    - PickupOrientation
- Timer compatibility test
    - (not checked because of defects of original implantation)
- Runtime reference replacement for spawned object to support uGUI


## Defects and known issues
- Player collider layer miss setuped?
    - Emu_Player prefab has Colliders/{PlayerLocalLayer, PlayerLayer} . And in current implantation PlayerLayer is initially enabled. 
- NullReferenceException occurs when VRC_SceneDescriptor is missing


## Tuning

- Raycast cost (length, mask, ...)


## Idea

- Conditional "break"
    - Pause execution of trigger-action if some condition become true
- Test automation
    - Automaticall operation and asset condition
- VRC component access gateway
    - Concentrate dependancy at a point

---
end
