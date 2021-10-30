using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using GameWorkspace;
using Nethereum.Web3;

namespace NeoTokyo {

    // Raw data from JSON
    class RawAttribute {
        public string trait_type { get; set; }
        public JsonElement value { get; set; }
    }

    // Raw data from JSON
    class RawIdentity {
        public string name { get; set; }
        public RawAttribute[] attributes { get; set; }
        public string description { get; set; }
        public string image { get; set; }

        public RawAttribute AtributeWithTraitType(string traitType) {
            return Array.Find<RawAttribute>(this.attributes, attribute => attribute.trait_type == traitType);
        }
    }

    // Class containing all the data relative to an identity.
    class Identity {
        public string Name;
        public string Status;
        public string Class;
        public string Gender;
        public string Race;
        public string Ability;
        public string Eyes;
        public int Strength;
        public int Intelligence;
        public int Attractiveness;
        public int TechSkill;
        public int Cool;
        public int Credits;
        public string CreditYield;
        public string ImageData;
        public string RawJson;

        public Identity(RawIdentity rawIdentity) {
            Name = rawIdentity.name;
            Status = rawIdentity.AtributeWithTraitType("Status").value.ToString();
            Class = rawIdentity.AtributeWithTraitType("Class").value.ToString();
            Gender = rawIdentity.AtributeWithTraitType("Gender").value.ToString();
            Race = rawIdentity.AtributeWithTraitType("Race").value.ToString();
            Ability = rawIdentity.AtributeWithTraitType("Ability").value.ToString();
            Eyes = rawIdentity.AtributeWithTraitType("Eyes").value.ToString();
            rawIdentity.AtributeWithTraitType("Strength").value.TryGetInt32(out Strength);
            rawIdentity.AtributeWithTraitType("Intelligence").value.TryGetInt32(out Intelligence);
            rawIdentity.AtributeWithTraitType("Attractiveness").value.TryGetInt32(out Attractiveness);
            rawIdentity.AtributeWithTraitType("Tech Skill").value.TryGetInt32(out TechSkill);
            rawIdentity.AtributeWithTraitType("Cool").value.TryGetInt32(out Cool);
            rawIdentity.AtributeWithTraitType("Credits").value.TryGetInt32(out Credits);
            CreditYield = rawIdentity.AtributeWithTraitType("Credit Yield").value.ToString();
            ImageData = Encoding.UTF8.GetString(Convert.FromBase64String(rawIdentity.image.Split(",")[1]));
            // TODO: extract rarity (rare, epic, legendary) from the ImageData.
        }

        public String toCSV() {
            return Name + "," + Status + "," + Class + "," + Gender + "," + Race + "," + Ability + "," + Eyes + "," + Strength + "," + Intelligence + "," + Attractiveness + "," + TechSkill + "," + Cool + "," + Credits + "," + CreditYield;
        }
    }

    class Program
    {
        const string infuraProjectID = "3b73dc5e45b349fca22756706c4ca486";

        // Main
        // const string neoTokyoContract = "0xc47ae0a3fede6a15ad0d586baf76ceda0719a864";

        // Oct-07-2021 07:44:41 AM +UTC
        // const string neoTokyoContract = "064327e11893e812f62056795dfd55a1162d983d";

        // Oct-05-2021 09:32:03 PM +UTC
        const string neoTokyoContract = "0xc47ae0a3fede6a15ad0d586baf76ceda0719a864";


        const string abi = @"[{'inputs':[],'stateMutability':'nonpayable','type':'constructor'},{'anonymous':false,'inputs':[{'indexed':true,'internalType':'address','name':'owner','type':'address'},{'indexed':true,'internalType':'address','name':'approved','type':'address'},{'indexed':true,'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'Approval','type':'event'},{'anonymous':false,'inputs':[{'indexed':true,'internalType':'address','name':'owner','type':'address'},{'indexed':true,'internalType':'address','name':'operator','type':'address'},{'indexed':false,'internalType':'bool','name':'approved','type':'bool'}],'name':'ApprovalForAll','type':'event'},{'anonymous':false,'inputs':[{'indexed':true,'internalType':'address','name':'previousOwner','type':'address'},{'indexed':true,'internalType':'address','name':'newOwner','type':'address'}],'name':'OwnershipTransferred','type':'event'},{'anonymous':false,'inputs':[{'indexed':true,'internalType':'address','name':'from','type':'address'},{'indexed':true,'internalType':'address','name':'to','type':'address'},{'indexed':true,'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'Transfer','type':'event'},{'inputs':[{'internalType':'address','name':'to','type':'address'},{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'approve','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'owner','type':'address'}],'name':'balanceOf','outputs':[{'internalType':'uint256','name':'','type':'uint256'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'claim','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'getAbility','outputs':[{'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'getApproved','outputs':[{'internalType':'address','name':'','type':'address'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'getAttractiveness','outputs':[{'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'getClass','outputs':[{'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'getCool','outputs':[{'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'getCredits','outputs':[{'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'getEyes','outputs':[{'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'getGender','outputs':[{'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'getIntelligence','outputs':[{'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'getRace','outputs':[{'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'getStrength','outputs':[{'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'getTechSkill','outputs':[{'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'address','name':'_to','type':'address'},{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'handClaim','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'owner','type':'address'},{'internalType':'address','name':'operator','type':'address'}],'name':'isApprovedForAll','outputs':[{'internalType':'bool','name':'','type':'bool'}],'stateMutability':'view','type':'function'},{'inputs':[],'name':'name','outputs':[{'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function'},{'inputs':[],'name':'owner','outputs':[{'internalType':'address','name':'','type':'address'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'ownerClaim','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'ownerOf','outputs':[{'internalType':'address','name':'','type':'address'}],'stateMutability':'view','type':'function'},{'inputs':[],'name':'renounceOwnership','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'_to','type':'address'},{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'riddleClaim','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'from','type':'address'},{'internalType':'address','name':'to','type':'address'},{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'safeTransferFrom','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'from','type':'address'},{'internalType':'address','name':'to','type':'address'},{'internalType':'uint256','name':'tokenId','type':'uint256'},{'internalType':'bytes','name':'_data','type':'bytes'}],'name':'safeTransferFrom','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'operator','type':'address'},{'internalType':'bool','name':'approved','type':'bool'}],'name':'setApprovalForAll','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'contractAddress','type':'address'}],'name':'setHandMintContract','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'contractAddress','type':'address'}],'name':'setMintContract','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'contractAddress','type':'address'}],'name':'setRareMintContract','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[],'name':'setSale','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[],'name':'setWhitelistState','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'bytes4','name':'interfaceId','type':'bytes4'}],'name':'supportsInterface','outputs':[{'internalType':'bool','name':'','type':'bool'}],'stateMutability':'view','type':'function'},{'inputs':[],'name':'symbol','outputs':[{'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'index','type':'uint256'}],'name':'tokenByIndex','outputs':[{'internalType':'uint256','name':'','type':'uint256'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'address','name':'owner','type':'address'},{'internalType':'uint256','name':'index','type':'uint256'}],'name':'tokenOfOwnerByIndex','outputs':[{'internalType':'uint256','name':'','type':'uint256'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'tokenURI','outputs':[{'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function'},{'inputs':[],'name':'totalSupply','outputs':[{'internalType':'uint256','name':'','type':'uint256'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'address','name':'from','type':'address'},{'internalType':'address','name':'to','type':'address'},{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'transferFrom','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'newOwner','type':'address'}],'name':'transferOwnership','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'},{'internalType':'uint256','name':'spotInWhitelist','type':'uint256'},{'internalType':'bytes32[]','name':'proof','type':'bytes32[]'}],'name':'whitelistClaim','outputs':[],'stateMutability':'nonpayable','type':'function'}]";

        static void Main(string[] args)
        {
            GetIdentitites().Wait();
            Console.ReadLine();
        }

        static async Task GetIdentitites() {
            var web3 = new Web3("https://mainnet.infura.io/v3/" + infuraProjectID);
            var contract = web3.Eth.GetContract(abi, neoTokyoContract);
            var getUri = contract.GetFunction("tokenURI");

            Identity[] identities = new Identity[2500];
            String fullExport = "Name,Status,Class,Gender,Race,Ability,Eyes,Strength,Intelligence,Attractiveness,Tech Skill,Cool,Credits,Credit Yield";

            for (int i = 0; i < 2500; i++) {
                identities[i] = await GetIdentity(getUri, i);
                if (identities[i] == null) continue;
                File.WriteAllText(Environment.CurrentDirectory + "/CharacterStorage/Character-" + i + ".json", identities[i].RawJson);
                File.WriteAllText(Environment.CurrentDirectory + "/CharacterStorage/Character-" + i + ".svg", identities[i].ImageData);
                fullExport += "\n" + identities[i].toCSV();
            }

            File.WriteAllText(Environment.CurrentDirectory + "/CharacterStorage/Characters.xlsx", fullExport);

        }

        static async Task<Identity> GetIdentity(Nethereum.Contracts.Function getUri, int id) {
            string uri;
            try {
                uri = (await getUri.CallAsync<string>(id)).Split(",")[1];
            }
            catch (Nethereum.JsonRpc.Client.RpcResponseException) {
                Console.WriteLine("Token with ID #" + id + " not found.");
                return null;
            }

            Console.WriteLine("Processing: Citizen #" + id);

            string decodedString = Encoding.UTF8.GetString(Convert.FromBase64String(uri));
            var rawIdentity = JsonSerializer.Deserialize<RawIdentity>(decodedString);
            var identity = new Identity(rawIdentity);
            identity.RawJson = decodedString;

            return identity;
        }
    }
}
