using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using GameWorkspace;
using Nethereum.Web3;
using RestSharp;

namespace NeoTokyo
{

    // Raw data from JSON
    class RawAttribute
    {
        public string trait_type { get; set; }
        public JsonElement value { get; set; }
    }

    // Raw data from JSON
    class RawValue
    {
        public string name { get; set; }
        public RawAttribute[] attributes { get; set; }
        public string description { get; set; }
        public string image { get; set; }

        public RawAttribute AtributeWithTraitType(string traitType)
        {
            return Array.Find<RawAttribute>(this.attributes, attribute => attribute.trait_type == traitType);
        }
    }

    // Class containing all the data relative to an identity.
    class Identity
    {
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
        public bool OpenedVault;
        public int Rarity;

        public Identity(RawValue rawIdentity)
        {
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

        public String toCSV()
        {
            return Name + "," + Status + "," + Class + "," + Gender + "," + Race + "," + Ability + "," + Eyes + "," + Strength + "," + Intelligence + "," + Attractiveness + "," + TechSkill + "," + Cool + "," + Credits + "," + CreditYield + "," + OpenedVault + "," + Rarity;
        }
    }

    class Vault
    {
        public int Credits;
        public String CreditSupplyProportion;
        public String AdditionalItem;
        public String CreditMultiplier;
        public string ImageData;
        public string RawJson;
        public int openedBy;

        public Vault(RawValue rawVault)
        {
            rawVault.AtributeWithTraitType("Credits").value.TryGetInt32(out Credits);
            CreditSupplyProportion = rawVault.AtributeWithTraitType("Credit Supply Proportion").value.ToString();
            AdditionalItem = rawVault.AtributeWithTraitType("Additional Item").value.ToString();
            CreditMultiplier = rawVault.AtributeWithTraitType("Credit Multiplier").value.ToString();
            ImageData = Encoding.UTF8.GetString(Convert.FromBase64String(rawVault.image.Split(",")[1]));
        }

        public String toCSV()
        {
            return Credits + "," + CreditSupplyProportion + "," + AdditionalItem + "," + CreditMultiplier + "," + openedBy;
        }
    }

    class Program
    {
        const string infuraProjectID = "3b73dc5e45b349fca22756706c4ca486";
        const string neoTokyoContract = "0x86357A19E5537A8Fba9A004E555713BC943a66C0";
        const string abi = @"[{'inputs':[],'stateMutability':'nonpayable','type':'constructor'},{'anonymous':false,'inputs':[{'indexed':true,'internalType':'address','name':'owner','type':'address'},{'indexed':true,'internalType':'address','name':'approved','type':'address'},{'indexed':true,'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'Approval','type':'event'},{'anonymous':false,'inputs':[{'indexed':true,'internalType':'address','name':'owner','type':'address'},{'indexed':true,'internalType':'address','name':'operator','type':'address'},{'indexed':false,'internalType':'bool','name':'approved','type':'bool'}],'name':'ApprovalForAll','type':'event'},{'anonymous':false,'inputs':[{'indexed':true,'internalType':'address','name':'previousOwner','type':'address'},{'indexed':true,'internalType':'address','name':'newOwner','type':'address'}],'name':'OwnershipTransferred','type':'event'},{'anonymous':false,'inputs':[{'indexed':true,'internalType':'address','name':'from','type':'address'},{'indexed':true,'internalType':'address','name':'to','type':'address'},{'indexed':true,'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'Transfer','type':'event'},{'inputs':[{'internalType':'address','name':'to','type':'address'},{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'approve','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'owner','type':'address'}],'name':'balanceOf','outputs':[{'internalType':'uint256','name':'','type':'uint256'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'claim','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'getAbility','outputs':[{'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'getApproved','outputs':[{'internalType':'address','name':'','type':'address'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'getAttractiveness','outputs':[{'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'getClass','outputs':[{'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'getCool','outputs':[{'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'getCredits','outputs':[{'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'getEyes','outputs':[{'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'getGender','outputs':[{'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'getIntelligence','outputs':[{'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'getRace','outputs':[{'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'getStrength','outputs':[{'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'getTechSkill','outputs':[{'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'address','name':'_to','type':'address'},{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'handClaim','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'owner','type':'address'},{'internalType':'address','name':'operator','type':'address'}],'name':'isApprovedForAll','outputs':[{'internalType':'bool','name':'','type':'bool'}],'stateMutability':'view','type':'function'},{'inputs':[],'name':'name','outputs':[{'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function'},{'inputs':[],'name':'owner','outputs':[{'internalType':'address','name':'','type':'address'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'ownerClaim','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'ownerOf','outputs':[{'internalType':'address','name':'','type':'address'}],'stateMutability':'view','type':'function'},{'inputs':[],'name':'renounceOwnership','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'_to','type':'address'},{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'riddleClaim','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'from','type':'address'},{'internalType':'address','name':'to','type':'address'},{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'safeTransferFrom','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'from','type':'address'},{'internalType':'address','name':'to','type':'address'},{'internalType':'uint256','name':'tokenId','type':'uint256'},{'internalType':'bytes','name':'_data','type':'bytes'}],'name':'safeTransferFrom','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'operator','type':'address'},{'internalType':'bool','name':'approved','type':'bool'}],'name':'setApprovalForAll','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'contractAddress','type':'address'}],'name':'setHandMintContract','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'contractAddress','type':'address'}],'name':'setMintContract','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'contractAddress','type':'address'}],'name':'setRareMintContract','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[],'name':'setSale','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[],'name':'setWhitelistState','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'bytes4','name':'interfaceId','type':'bytes4'}],'name':'supportsInterface','outputs':[{'internalType':'bool','name':'','type':'bool'}],'stateMutability':'view','type':'function'},{'inputs':[],'name':'symbol','outputs':[{'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'index','type':'uint256'}],'name':'tokenByIndex','outputs':[{'internalType':'uint256','name':'','type':'uint256'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'address','name':'owner','type':'address'},{'internalType':'uint256','name':'index','type':'uint256'}],'name':'tokenOfOwnerByIndex','outputs':[{'internalType':'uint256','name':'','type':'uint256'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'tokenURI','outputs':[{'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function'},{'inputs':[],'name':'totalSupply','outputs':[{'internalType':'uint256','name':'','type':'uint256'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'address','name':'from','type':'address'},{'internalType':'address','name':'to','type':'address'},{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'transferFrom','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'newOwner','type':'address'}],'name':'transferOwnership','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'},{'internalType':'uint256','name':'spotInWhitelist','type':'uint256'},{'internalType':'bytes32[]','name':'proof','type':'bytes32[]'}],'name':'whitelistClaim','outputs':[],'stateMutability':'nonpayable','type':'function'}]";

        const string vaultContract = "0xab0b0dd7e4eab0f9e31a539074a03f1c1be80879";
        const string vaultAbi = "[{'inputs':[],'stateMutability':'nonpayable','type':'constructor'},{'anonymous':false,'inputs':[{'indexed':true,'internalType':'address','name':'owner','type':'address'},{'indexed':true,'internalType':'address','name':'approved','type':'address'},{'indexed':true,'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'Approval','type':'event'},{'anonymous':false,'inputs':[{'indexed':true,'internalType':'address','name':'owner','type':'address'},{'indexed':true,'internalType':'address','name':'operator','type':'address'},{'indexed':false,'internalType':'bool','name':'approved','type':'bool'}],'name':'ApprovalForAll','type':'event'},{'anonymous':false,'inputs':[{'indexed':true,'internalType':'address','name':'previousOwner','type':'address'},{'indexed':true,'internalType':'address','name':'newOwner','type':'address'}],'name':'OwnershipTransferred','type':'event'},{'anonymous':false,'inputs':[{'indexed':true,'internalType':'address','name':'from','type':'address'},{'indexed':true,'internalType':'address','name':'to','type':'address'},{'indexed':true,'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'Transfer','type':'event'},{'inputs':[{'internalType':'address','name':'to','type':'address'},{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'approve','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'owner','type':'address'}],'name':'balanceOf','outputs':[{'internalType':'uint256','name':'','type':'uint256'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'claim','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'getAdditionalItem','outputs':[{'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'getApproved','outputs':[{'internalType':'address','name':'','type':'address'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'getClaimantIdentityIdByTokenId','outputs':[{'internalType':'uint256','name':'','type':'uint256'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'getCreditMultiplier','outputs':[{'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'getCreditProportionOfTotalSupply','outputs':[{'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'getCredits','outputs':[{'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'identityTokenId','type':'uint256'}],'name':'getTokenClaimedByIdentityTokenId','outputs':[{'internalType':'uint256','name':'','type':'uint256'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'holderClaim','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'owner','type':'address'},{'internalType':'address','name':'operator','type':'address'}],'name':'isApprovedForAll','outputs':[{'internalType':'bool','name':'','type':'bool'}],'stateMutability':'view','type':'function'},{'inputs':[],'name':'name','outputs':[{'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function'},{'inputs':[],'name':'owner','outputs':[{'internalType':'address','name':'','type':'address'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'ownerClaim','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'ownerOf','outputs':[{'internalType':'address','name':'','type':'address'}],'stateMutability':'view','type':'function'},{'inputs':[],'name':'renounceOwnership','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'from','type':'address'},{'internalType':'address','name':'to','type':'address'},{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'safeTransferFrom','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'from','type':'address'},{'internalType':'address','name':'to','type':'address'},{'internalType':'uint256','name':'tokenId','type':'uint256'},{'internalType':'bytes','name':'_data','type':'bytes'}],'name':'safeTransferFrom','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'operator','type':'address'},{'internalType':'bool','name':'approved','type':'bool'}],'name':'setApprovalForAll','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'contractAddress','type':'address'}],'name':'setContract','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'contractAddress','type':'address'}],'name':'setIdAddress','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[],'name':'setOpenClaimState','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[],'name':'setSale','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[],'name':'setWhitelistState','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'bytes4','name':'interfaceId','type':'bytes4'}],'name':'supportsInterface','outputs':[{'internalType':'bool','name':'','type':'bool'}],'stateMutability':'view','type':'function'},{'inputs':[],'name':'symbol','outputs':[{'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'index','type':'uint256'}],'name':'tokenByIndex','outputs':[{'internalType':'uint256','name':'','type':'uint256'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'address','name':'owner','type':'address'},{'internalType':'uint256','name':'index','type':'uint256'}],'name':'tokenOfOwnerByIndex','outputs':[{'internalType':'uint256','name':'','type':'uint256'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'tokenURI','outputs':[{'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function'},{'inputs':[],'name':'totalSupply','outputs':[{'internalType':'uint256','name':'','type':'uint256'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'address','name':'from','type':'address'},{'internalType':'address','name':'to','type':'address'},{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'transferFrom','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'newOwner','type':'address'}],'name':'transferOwnership','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'},{'internalType':'uint256','name':'spotInWhitelist','type':'uint256'},{'internalType':'bytes32[]','name':'proof','type':'bytes32[]'}],'name':'whitelistClaim','outputs':[],'stateMutability':'nonpayable','type':'function'}]";

        const string bytesContract = "0x7d647b1A0dcD5525e9C6B3D14BE58f27674f8c95";
        const string bytesAbi = "[{'inputs':[],'stateMutability':'nonpayable','type':'constructor'},{'anonymous':false,'inputs':[{'indexed':true,'internalType':'address','name':'owner','type':'address'},{'indexed':true,'internalType':'address','name':'spender','type':'address'},{'indexed':false,'internalType':'uint256','name':'value','type':'uint256'}],'name':'Approval','type':'event'},{'anonymous':false,'inputs':[{'indexed':true,'internalType':'address','name':'previousOwner','type':'address'},{'indexed':true,'internalType':'address','name':'newOwner','type':'address'}],'name':'OwnershipTransferred','type':'event'},{'anonymous':false,'inputs':[{'indexed':true,'internalType':'address','name':'user','type':'address'},{'indexed':false,'internalType':'uint256','name':'reward','type':'uint256'}],'name':'RewardPaid','type':'event'},{'anonymous':false,'inputs':[{'indexed':true,'internalType':'address','name':'from','type':'address'},{'indexed':true,'internalType':'address','name':'to','type':'address'},{'indexed':false,'internalType':'uint256','name':'value','type':'uint256'}],'name':'Transfer','type':'event'},{'inputs':[{'internalType':'address','name':'_address','type':'address'}],'name':'addAdminContractAddress','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'','type':'address'}],'name':'adminContracts','outputs':[{'internalType':'bool','name':'','type':'bool'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'address','name':'owner','type':'address'},{'internalType':'address','name':'spender','type':'address'}],'name':'allowance','outputs':[{'internalType':'uint256','name':'','type':'uint256'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'address','name':'spender','type':'address'},{'internalType':'uint256','name':'amount','type':'uint256'}],'name':'approve','outputs':[{'internalType':'bool','name':'','type':'bool'}],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'account','type':'address'}],'name':'balanceOf','outputs':[{'internalType':'uint256','name':'','type':'uint256'}],'stateMutability':'view','type':'function'},{'inputs':[],'name':'boughtIdentityContract','outputs':[{'internalType':'contract IIdentity','name':'','type':'address'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'address','name':'_from','type':'address'},{'internalType':'uint256','name':'_amount','type':'uint256'}],'name':'burn','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[],'name':'citizenContract','outputs':[{'internalType':'contract ICitizen','name':'','type':'address'}],'stateMutability':'view','type':'function'},{'inputs':[],'name':'decimals','outputs':[{'internalType':'uint8','name':'','type':'uint8'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'address','name':'spender','type':'address'},{'internalType':'uint256','name':'subtractedValue','type':'uint256'}],'name':'decreaseAllowance','outputs':[{'internalType':'bool','name':'','type':'bool'}],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'_to','type':'address'}],'name':'getReward','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'_user','type':'address'}],'name':'getTotalClaimable','outputs':[{'internalType':'uint256','name':'','type':'uint256'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'hasIdentityOpenedABox','outputs':[{'internalType':'bool','name':'','type':'bool'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'hasVaultBoxBeenOpened','outputs':[{'internalType':'bool','name':'','type':'bool'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'','type':'uint256'}],'name':'identityBoxOpened','outputs':[{'internalType':'uint256','name':'','type':'uint256'}],'stateMutability':'view','type':'function'},{'inputs':[],'name':'identityContract','outputs':[{'internalType':'contract IIdentity','name':'','type':'address'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'address','name':'spender','type':'address'},{'internalType':'uint256','name':'addedValue','type':'uint256'}],'name':'increaseAllowance','outputs':[{'internalType':'bool','name':'','type':'bool'}],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'','type':'address'}],'name':'lastUpdate','outputs':[{'internalType':'uint256','name':'','type':'uint256'}],'stateMutability':'view','type':'function'},{'inputs':[],'name':'name','outputs':[{'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'identityTokenId','type':'uint256'},{'internalType':'uint256','name':'vaultBoxTokenId','type':'uint256'}],'name':'openVaultBox','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[],'name':'owner','outputs':[{'internalType':'address','name':'','type':'address'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'address','name':'_address','type':'address'}],'name':'removeAdminContactAddress','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[],'name':'renounceOwnership','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'','type':'address'}],'name':'rewards','outputs':[{'internalType':'uint256','name':'','type':'uint256'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'address','name':'_address','type':'address'}],'name':'setBoughtIdentityContract','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'_address','type':'address'}],'name':'setCitizenContract','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'_address','type':'address'}],'name':'setIdentityContract','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'_address','type':'address'}],'name':'setVaultBoxContract','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'string','name':'_str','type':'string'}],'name':'strToUint','outputs':[{'internalType':'uint256','name':'res','type':'uint256'},{'internalType':'bool','name':'err','type':'bool'}],'stateMutability':'pure','type':'function'},{'inputs':[],'name':'symbol','outputs':[{'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function'},{'inputs':[],'name':'totalSupply','outputs':[{'internalType':'uint256','name':'','type':'uint256'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'address','name':'recipient','type':'address'},{'internalType':'uint256','name':'amount','type':'uint256'}],'name':'transfer','outputs':[{'internalType':'bool','name':'','type':'bool'}],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'sender','type':'address'},{'internalType':'address','name':'recipient','type':'address'},{'internalType':'uint256','name':'amount','type':'uint256'}],'name':'transferFrom','outputs':[{'internalType':'bool','name':'','type':'bool'}],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'newOwner','type':'address'}],'name':'transferOwnership','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'uint256','name':'_amount','type':'uint256'}],'name':'updateMaxRewardableTokens','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'_from','type':'address'},{'internalType':'address','name':'_to','type':'address'},{'internalType':'uint256','name':'_tokenId','type':'uint256'}],'name':'updateReward','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'_user','type':'address'},{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'updateRewardOnMint','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[],'name':'vaultBoxContract','outputs':[{'internalType':'contract IVaultBox','name':'','type':'address'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'','type':'uint256'}],'name':'vaultBoxOpenedByIdentity','outputs':[{'internalType':'uint256','name':'','type':'uint256'}],'stateMutability':'view','type':'function'}]";

        static void Main(string[] args)
        {
            var web3 = new Web3("https://mainnet.infura.io/v3/" + infuraProjectID);
            //GetVaults(web3).Wait();
            GetIdentitites(web3).Wait();
            //GetIdentityPrice(1793);
            Console.ReadLine();
        }

        static async Task GetIdentitites(Web3 web3)
        {
            Identity[] identities = new Identity[2500];
            String fullExport = "Name,Status,Class,Gender,Race,Ability,Eyes,Strength,Intelligence,Attractiveness,Tech Skill,Cool,Credits,Credit Yield,Opened Vault,Rarity";

            for (int i = 0; i < 2500; i++)
            {
                identities[i] = await GetIdentity(web3, i);
                if (identities[i] == null) continue;
                File.WriteAllText(Environment.CurrentDirectory + "/CharacterStorage/Character-" + i + ".json", identities[i].RawJson);
                File.WriteAllText(Environment.CurrentDirectory + "/CharacterStorage/Character-" + i + ".svg", identities[i].ImageData);
                fullExport += "\n" + identities[i].toCSV();
            }

            File.WriteAllText(Environment.CurrentDirectory + "/CharacterStorage/Characters.xlsx", fullExport);

        }

        static async Task<Identity> GetIdentity(Web3 web3, int id)
        {
            var contract = web3.Eth.GetContract(abi, neoTokyoContract);
            var getUri = contract.GetFunction("tokenURI");

            string uri;
            try
            {
                uri = (await getUri.CallAsync<string>(id)).Split(",")[1];
            }
            catch (Nethereum.JsonRpc.Client.RpcResponseException)
            {
                Console.WriteLine("Token with ID #" + id + " not found.");
                return null;
            }

            Console.WriteLine("Processing: Citizen #" + id);

            string decodedString = Encoding.UTF8.GetString(Convert.FromBase64String(uri));
            var rawIdentity = JsonSerializer.Deserialize<RawValue>(decodedString);
            var identity = new Identity(rawIdentity);
            identity.RawJson = decodedString;
            identity.Rarity = GetIdentityRarity(id);

            var contract2 = web3.Eth.GetContract(bytesAbi, bytesContract);
            var hasOpenedVault = contract2.GetFunction("hasIdentityOpenedABox");

            bool openedVault;
            try
            {
                openedVault = (await hasOpenedVault.CallAsync<bool>(id));
            }
            catch (Nethereum.JsonRpc.Client.RpcResponseException)
            {
                Console.WriteLine("Token with ID #" + id + " not found.");
                return null;
            }

            identity.OpenedVault = openedVault;

            return identity;
        }

        static int GetIdentityRarity(int id)
        {
            var url = "https://raritymon.com/Item-details?collection=neotokyo&id=" + id;
            string s;

            using (WebClient client = new WebClient())
            {
                s = client.DownloadString(url);
            }

            int start = s.IndexOf("item-rarity-rank");
            string substring = s.Substring(start);
            string rankString = substring.Split(" ")[3];
            int rank = int.Parse(rankString);

            return rank;
        }

        static void GetIdentityPrice(int id)
        {
            var client = new RestClient("https://api.opensea.io/api/v1/asset/0x86357a19e5537a8fba9a004e555713bc943a66c0/" + id + "/");
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);

            dynamic result = JsonSerializer.Deserialize<dynamic>(response.Content);
            // current_price
        }

        /*static float GetIdentityPrice(int id)
        {
            var url = "https://opensea.io/assets/0x86357a19e5537a8fba9a004e555713bc943a66c0/" + id;
            string s;

            using (WebClient client = new WebClient())
            {
                s = client.DownloadString(url);
            }

            int start = s.IndexOf("Price--amount");
            string substring = s.Substring(start);
            string priceString = substring.Split(" ")[3];
            float price = float.Parse(priceString);

            return price;
        }*/

        static async Task GetVaults(Web3 web3)
        {
            var contract = web3.Eth.GetContract(abi, neoTokyoContract);

            Vault[] vaults = new Vault[2500];
            String fullExport = "Credits,CreditSupplyProportion,AdditionalItem,CreditMultiplier,OpenedBy";

            for (int i = 0; i < 2500; i++)
            {
                vaults[i] = await GetVault(web3, i);
                if (vaults[i] == null) continue;
                File.WriteAllText(Environment.CurrentDirectory + "/CharacterStorage/Vault-" + i + ".json", vaults[i].RawJson);
                File.WriteAllText(Environment.CurrentDirectory + "/CharacterStorage/Vault-" + i + ".svg", vaults[i].ImageData);
                fullExport += "\n" + vaults[i].toCSV();
            }

            File.WriteAllText(Environment.CurrentDirectory + "/CharacterStorage/Vaults.xlsx", fullExport);

        }

        static async Task<Vault> GetVault(Web3 web3, int id)
        {
            var contract = web3.Eth.GetContract(vaultAbi, vaultContract);
            var getUri = contract.GetFunction("tokenURI");

            string uri;
            try
            {
                uri = (await getUri.CallAsync<string>(id)).Split(",")[1];
            }
            catch (Nethereum.JsonRpc.Client.RpcResponseException)
            {
                Console.WriteLine("Token with ID #" + id + " not found.");
                return null;
            }

            Console.WriteLine("Processing: Vault #" + id);

            string decodedString = Encoding.UTF8.GetString(Convert.FromBase64String(uri));
            var rawVault = JsonSerializer.Deserialize<RawValue>(decodedString);
            var vault = new Vault(rawVault);
            vault.RawJson = decodedString;

            var contract2 = web3.Eth.GetContract(bytesAbi, bytesContract);
            var hasBeenOpened = contract2.GetFunction("vaultBoxOpenedByIdentity");

            int openedBy;
            try
            {
                openedBy = (await hasBeenOpened.CallAsync<int>(id));
            }
            catch (Nethereum.JsonRpc.Client.RpcResponseException)
            {
                Console.WriteLine("Token with ID #" + id + " not found.");
                return null;
            }

            vault.openedBy = openedBy;

            return vault;
        }
    }
}
