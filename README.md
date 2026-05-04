# VR Driving Project With Motion Platform Actuation

![Unity](https://img.shields.io/badge/Unity-6000.3.10f1-black)
![VR](https://img.shields.io/badge/VR-SteamVR-green)
![License](https://img.shields.io/badge/License-MIT-lightgrey)

---

## Overview

Unity driving simulation with VR support.

Simplified driving model with one-pedal drive for acceleration and braking.

Steering includes a noticeable deadzone. No fix available at the moment.

Supports motion platforms.

---

## Features

- Drivable vehicle  
- Adjustable physics  
- Motion platform support (DOF Reality)  
- MOZA Racing support (steering and pedals)  

---

## Requirements

- Unity 6 (tested with 6000.3.10f1)  
- SteamVR  
- DOF Reality motion platform required for motion support (tested with H3 platform)  

---

## Installation

1. Clone or download this repository  
2. Open Unity Hub  
3. Add project  
4. Open project  

---

## First Launch

Open scene:  
`Assets > Snow mountain track > s1`

<img width="1465" height="429" alt="image" src="https://github.com/user-attachments/assets/a1b7490a-08f3-427d-b1d1-f4380b3d3645" />

Expected result:

![Scene](https://github.com/user-attachments/assets/b57f64c0-46e0-4242-8ac0-ff8d559f4de7)

### Fix Missing Textures

1. Open  
   `Window > Rendering > Render Pipeline Converter`  
2. Run all converters  
3. Apply fixes  

---

## Controls

- Use steering wheel shifters. Right shifter selects forward gear. Left shifter selects reverse gear.  
- Use brake and throttle pedals.  
- Press the **START** button on the steering wheel to teleport to the starting position.  
- Press **R** on the keyboard to teleport to the starting position.  
- Press **0** on the keyboard to reset the distance counter.  

---

## Vehicle Setup

### Adjust Settings

1. Select vehicle  
2. Open **Car Controller**  

![Controller](https://github.com/user-attachments/assets/771ac8e2-97b5-4db2-b0dc-3739336fddfc)

### Edit Scripts

Path:  
`Assets > Ezereal Truck > Ezereal Car Controller > Scripts`

![Scripts](https://github.com/user-attachments/assets/2442fe5e-9884-4e30-8282-f2689a799ff0)

---

## VR Setup

1. Install SteamVR  
2. Start SteamVR  
3. Connect headset  
4. Press Play in Unity  

VR runs through SteamVR. SteamVR must be running and the headset must be connected before starting.

---

## Motion Platform Setup

Supports DOF Reality.

- Attach motion scripts to the vehicle body  
  - Telemetry script  
  - Sim Racing Studio script  
- Use the same GameObject as movement reference   

![Motion](https://github.com/user-attachments/assets/e8823246-6f5a-4690-99e8-945012a23472)

---

## Assets Used

- Ezereal Car Controller  
  https://assetstore.unity.com/packages/tools/physics/ezereal-car-controller-302577  

- Engine Sound  
  https://assetstore.unity.com/packages/audio/sound-fx/transportation/i6-german-free-engine-sound-pack-106037  

- Mountain Tracks  
  https://assetstore.unity.com/packages/3d/environments/landscapes/mountain-race-tracks-110408  

- EasyRoads3D  
  https://assetstore.unity.com/packages/3d/characters/easyroads3d-free-v3-987  

- Road Pack  
  https://assetstore.unity.com/packages/3d/environments/roadways/low-poly-road-pack-created-with-fastmesh-asset-293643  

- VR Hands  
  https://assetstore.unity.com/packages/3d/characters/stylized-simple-hands-221297  

---

## License

MIT-based components used.  
See `Licenses` folder.

### GingerVR
https://github.com/angsamuel/GingerVR  
MIT License  
See `Licenses/GingerVR_MIT.txt`

### SRS API
https://gitlab.com/simracingstudio/srsapi  
MIT License  
See `Licenses/SRSAPI_MIT.txt`
