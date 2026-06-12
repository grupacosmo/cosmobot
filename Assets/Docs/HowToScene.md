# Scene

---

# Important
After starting any scene, the [GameManagerScene](../Scenes/GameManagerScene.unity) is automatically loaded additionally and is kept loaded for all time.

This behaviour is controlled by [GameManagerSceneLoader](../Scripts/Utils/GameManagerSceneLoader.cs).

# Ready to use scenes
Folder: [Assets/Scenes](../Scenes)

- [MainTestScene](../Scenes/0_MainTestScene.unity) - Scene with all working features. Some features on this scene can be still in process, but only if they can be used without causing any issues
- [Demo/FullTest](../Scenes/Demo/FullTest.unity) - Scene for presenting Demo version of game (in progress)

## Other scenes
- [Folder Demo](../Scenes/Demo) - Demo scenes for presentations
- [Folder FeatureExamples](../Scenes/FeatureExamples) - Isolated features to be tested/presented in controlled environment (without other non required features) 

# Prefabs
Folder: [Assets/Prefabs](../Prefabs)

There are planty of prefabs available, these are the most useful to set up new scene from scratch: 
- [Player/PlayerBase](../Prefabs/Player/PlayerBase.prefab) - Fully functional player with all in game functionality like building, interactions etc.
  - Please do not move prefab's root GameObject, it should stay on `(0,0,0)` and is marked as static.  
- [Player/PlayerCharacter](../Prefabs/Player/PlayerCharacter.prefab) - Just a player character
  - able to move, pickup items, and interact (with limited ui)
  - without any other functionality like building, etc.
- [ItemSpawner](../Prefabs/ItemSpawner.prefab) - Use to put item on scene (will change into actual item on `Awake`)
  - Change `ItemInfo` property of `ItemSpawner` component to change spawned item.
  - Small cross billboard with item texture will appear in scene view
- [Folder Structures/Crafters](../Prefabs/Structures/Crafters) - various Crafters for crafting  