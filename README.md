# NeoTokyo
Neo Tokyo CODES investigations

The goal is to breakdown the code and understand what is the underlying message.
I try to make it readable for anyone, include those with very little coding background.

Analyzing the code.
So far we only have the first part: identity.

```
using System;
using System.IO;
using System.Text.Json;
```

Those are very standard libraries.

`IO` stands for "InputOutput".

`Json` is a library which allows to translate data to/from human readable formatted text file. This is the by default format used to exchange data between systems of different architecture.

```
using GameWorkspace.AuthUtils;
```

This library does not exist and we must assume this is developped in the scope of the NeoTokyo project. `AuthUtils` hint at something related to authentication.

```
class player
{
    private string characterClass = "Chat Support";
    private string characterGender = "Female";
    private string characterRace = "Ghost";
    private int strength = 75;
    private int intelligence = 59;
    private int attractiveness = 33;
    private int techSkill = 57;
    private int cool = 77;
    private int credits = 295;
    private string characterEyes = "Scar";
    private string creditYield = "Low";
    private string ability = "Shield";
}; //DAY ONE UPLOAD IDENTITY
```

This simply defines a `player` class with different attributes.

Now to the juicy part.

```
int main(string[] args)
{
    player uniqueCharacter = GenerateCharacter();
    ApplyCredits(uniqueCharacter);
    try
    {
        string json = JsonSerializer.Serialize(uniqueCharacter);
        if (json != null && json.Length > 0)
        {
            File.WriteAllText(Environment.CurrentDirectory + "/CharacterStorage/Character-" + tokenId.toString() + ".json", json;
        }
    }
    catch (SerializationException e) { }
    metaverseObjects.Add(uniqueCharacter);
    WebSocket = new WebSocket(wsLocation);
    Task _t = await connection.send("Uploading character data: " + json);
    Console.Writeline("Upload task status :" + _t.Status);
    while (!_t.IsCompleted) { Thread.Sleep(500); }
    Console.Writeline("Upload task completed");
    CreateCharacterInterfaceHeaders(uniqueCharacter, 0, 0, housingParams[], *networkArray);
    ValidateAuthToken(uniqueCharacter, housingParams[], *networkArray);
    VerifyCharacterLinkedList(**database.Storage.CLL.(uniqueCharacter), *networkArray);
    Console.Writeline(PlayerStateCheck(uniqueCharacter));
    Console.Writeline("Welcome Citizen!");
    return 0;
}
```

`main` is the entry point of any program.

The functions called here can be interpreted by their name as they are not existing standard c# functions.

The story being told here is this:

1. The player object is created.
2. Credits are applied to this player.
3. A file contianing the player data is written locally.
4. The player is added to the "metaverse" (array of objects).
5. A connection is opened `WebSocket` to a location `wsLocation`.
6. The data of the player is sent through this connection.
7. Some operations are run. Those are the most confusing from a developer perspective:
```
CreateCharacterInterfaceHeaders(uniqueCharacter, 0, 0, housingParams[], *networkArray);
ValidateAuthToken(uniqueCharacter, housingParams[], *networkArray);
VerifyCharacterLinkedList(**database.Storage.CLL.(uniqueCharacter), *networkArray);
```

Besides that there is a few `Console.Writeline` which is used to print some status. Very standard.

`try` and `catch` is standard for executing something which may fail. In this case this is step 3. above.

`return 0` simply means that the code has finished with no error.
