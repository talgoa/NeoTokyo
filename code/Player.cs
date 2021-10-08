namespace NeoTokyo
{
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

        public static player GenerateCharacter()
        {
            return new player();
        }

        /*static void Main(string[] args)
        {
            int tokenId = int.Parse(args[0]);

            player uniqueCharacter = player.GenerateCharacter();
            //ApplyCredits(uniqueCharacter);
            try
            {
                string json = JsonSerializer.Serialize(uniqueCharacter);
                if (json != null && json.Length > 0)
                {
                    File.WriteAllText(Environment.CurrentDirectory + "/CharacterStorage/Character-" + tokenId + ".json", json);
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
            //return 0;
        }*/
    }; //DAY ONE UPLOAD IDENTITY
}