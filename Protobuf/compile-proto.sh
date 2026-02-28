#!/bin/bash

project_root="<PROJECT_ROOT>"
bdsm_savegames="$project_root/BDSM/Runtime/SaveGames"
beta_models="$project_root/Beta/Extensions/Models"
beta_savegames="$project_root/Beta/Services/SaveGames/SaveGameDatas"

protoc -I=. --csharp_out="$beta_models" "./Vector2Surrogate.proto"

protoc -I=. --csharp_out="$bdsm_savegames" "./FieldSaveData.proto"

protoc -I=. --csharp_out="$beta_savegames" "./ActorSaveData.proto"
protoc -I=. --csharp_out="$beta_savegames" "./PlayerSaveData.proto"
protoc -I=. --csharp_out="$beta_savegames" "./SceneEntitySaveData.proto"
protoc -I=. --csharp_out="$beta_savegames" "./SceneSaveData.proto"

protoc -I=. --csharp_out="$beta_savegames" "./SaveGameData.proto"
