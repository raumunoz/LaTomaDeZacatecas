

==================================================================================
==================================================================================

==How To Use==
1. Check how the example is made or start from it, it's all setup for you.
2. To add values to save, you can either add properties to the existing MonoBehaviours in the SavedObject or add a MonoBehaviour in the SavedObject.
3. The start/default values of a save file will be the SavedObjects starting values.
4. Press Play, there is a save file selection field, a save button and a load buttons.
5. A SavedObject instance will appear in the scene, all the saved values are there. 
5. That's all you need to know, the rest is extra information in case you need it.

==================================================================================
==================================================================================


==Setup==
You should start from the example which is already setup, but if you don't want here's the process:
Here is the video tutorial for the setup: http://youtu.be/Bo1k07k4MR0
1. Create an empty GameObject and attach the SaveObject script to it.
2. Create the scripts that will contain the saved stats for your whole game and attach it to the GameObject you just created.
3. Press "Place in project" Button in the SaveObject inspector, it will delete the instance and put it as a prefab in the Resources Folder.
4. Adjust the values of the MonoBehaviour Scripts in the prefab you just created, it will be the default "New Game" state.
5. Press "Get Initialization Script" Button in the SaveObject inspector.
6. Paste (Ctrl+V) the script in the game initialization logic.
7. To test your setup, follow with ==Testing== instructions Step 2.


==Example==
The folder SaveObjectExample contains a fully explained and documented example (SaveObjectExample.cs) of the script implementation.
It also contains 2 examples of saved scripts (Progress.cs and ExampleStats.cs) and a scene with fully functional implementation.


==Testing==
1. Open the SaveObjectExampleScene.
2. Press play then select the "SaveObject" GameObject instance.
3. Change any value you want in the script inspectors or follow the GUI instructions.
4. Press Save.
5. Verify the game has indeed saved, load another game by repeating step 3, then reload the previous game by pressing "Load".
6. To know how to use the SaveObject component, please refer to ==Inspector==
7. To know how to use SaveObject script, please refer to ==Script Implementation==


==Inspector==
If you need to change values in the saved Scripts to test (example: get a lot of Golds to buy things), just change it.
"Loaded" or "Load" button: Will load the game described by the string next to it, when it is written "Load" you will load another game than the current one.
"Save" button: Will save the current game.
"Call Refresh" button: Will call the RefreshCallback. Please refer to the ==Refresh== section to know more.
"New Game" button: Will revert the current save state to the "New Game" state.


==Script Implementation==
Start with: SaveObject.Initialize(string or Gameobject);
Load with: SaveObject.Load(string);
Save with: SaveObject.Save();
Get a saved stat with: SaveObject.Get<T>(); Where T is the name of the script in which you want to access the stat.
Set a refresh callback with: SaveObject.SetRefreshCallback(Action);
Revert the game to a new game with: NewGame(); (The game won't be saved automatically so you need to save it if that's really what you want)


==Refresh==
Refresh calls an action you have previously set either with Initialize(); or SetRefreshCallback();
Refresh is automatically called when you load a game.
You can also manually call it with the button "Call Refresh".
The callback may be useful if you need to update stats that need special code to be reflected.

Example: You have a sound manager that plays music according to a setting "music volume" in a saved script, 
you change it manually in the inspector but the music volume will not update as you must also update the sound manager's volume.
So in the callback you update the sound manager's volume and press "Call Refresh" after changing the volume.


==Alternative Script Implementation==
If you don't want to use PlayerPrefs.
Use: SaveObject.GetSaveString(); to get the save string and use it as you see fit.
And: SaveObject.LoadFromString(string); to create the SaveObject from a saved string.


==WARNINGS==
Uses PlayerPrefs, if you want to store with other means, please refer to ==Alternative Script Implementation==.
Only one object per project can be saved/loaded at once, perfect to save persistent global stats but not optimized to save the state of different objects in a scene.
Cannot use a variable named "name" or any MonoBehaviour property in a saved component, it will silently fail and won't save properly.
Each session, the first time you load or save may cause a small hiccup (50ms) , the rest should be good.