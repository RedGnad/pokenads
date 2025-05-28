using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.Numerics;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

[Serializable]
public class GateCondition
{
    public enum Standard { ERC721, ERC1155 }
    public enum UnlockMode { AnyToken, SpecificToken }

    [Tooltip("ERC-721 ou ERC-1155")]
    public Standard standard = Standard.ERC1155;

    [Tooltip("AnyToken = n'importe quel jeton | SpecificToken = IDs listés")]
    public UnlockMode unlockMode = UnlockMode.AnyToken;

    [Tooltip("Adresse du contrat NFT")]
    public string contractAddress;

    [Tooltip("Liste des tokenIds (pour SpecificToken), ou vide pour AnyToken")]
    public List<string> tokenIds = new List<string>();

    [Tooltip("Boutons à déverrouiller si condition remplie")]
    public List<Button> targetButtons = new List<Button>();
}

public class NFTGateController : MonoBehaviour
{
    [Header("RPC & Collections")]
    [Tooltip("URL du RPC Monad Testnet")]
    public string rpcUrl = "https://testnet-rpc.monad.xyz";

    [Tooltip("Liste des collections et règles de gating")]
    public List<GateCondition> conditions = new List<GateCondition>();

    [Header("UI Buttons")]
    [Tooltip("Bouton ‘Armes’ ou équivalent pour lancer la vérification")]
    public Button checkButton;

    // Sélecteurs ABI et événement TransferSingle
    const string SEL_ERC1155_BALANCE = "0x00fdd58e";  
    const string SEL_ERC721_BALANCE  = "0x70a08231";  
    const string SEL_ERC721_OWNER    = "0x6352211e";  
    const string SIG_ERC1155_LOG     = "0xc3d58168c5ab16844f149d4b3945f6c6af9a1c1e0db3a9e6b207d0e2de5e2c8b";

    void Awake()
    {
        Debug.Log("[NFTGate][Awake] Locking all target buttons");
        foreach (var cond in conditions)
            foreach (var btn in cond.targetButtons)
                if (btn != null)
                {
                    btn.interactable = false;
                    Debug.Log($"[NFTGate][Awake] Locked button '{btn.name}' for contract {cond.contractAddress}");
                }
    }

    void Start()
    {
        if (checkButton != null)
        {
            Debug.Log($"[NFTGate][Start] Binding checkButton '{checkButton.name}'");
            checkButton.onClick.AddListener(() => StartCoroutine(CheckAll()));
        }
        else
        {
            Debug.LogWarning("[NFTGate][Start] checkButton not assigned!");
        }
    }

    IEnumerator CheckAll()
    {
        Debug.Log("[NFTGate][CheckAll] Starting verification");
        string wallet = WalletManager.CurrentWalletAddress;
        Debug.Log($"[NFTGate][CheckAll] Current wallet: {wallet}");
        if (string.IsNullOrEmpty(wallet))
        {
            Debug.LogWarning("[NFTGate][CheckAll] No wallet connected, aborting");
            yield break;
        }

        string ownerHex = wallet.StartsWith("0x")
            ? wallet.Substring(2).PadLeft(64, '0')
            : wallet.PadLeft(64, '0');
        Debug.Log($"[NFTGate][CheckAll] ownerHex: {ownerHex}");

        foreach (var cond in conditions)
        {
            Debug.Log($"[NFTGate][CheckAll] Rule: {cond.standard} / {cond.unlockMode} on {cond.contractAddress}");
            bool unlocked = false;

            if (cond.standard == GateCondition.Standard.ERC1155)
            {
                if (cond.unlockMode == GateCondition.UnlockMode.AnyToken)
                {
                    Debug.Log("[NFTGate][ERC1155|AnyToken] Scanning logs for any token...");
                    yield return StartCoroutine(CheckAnyTokenERC1155(
                        cond.contractAddress, wallet,
                        result => unlocked = result
                    ));
                    Debug.Log($"[NFTGate][ERC1155|AnyToken] Result = {unlocked}");
                }
                else
                {
                    foreach (var id in cond.tokenIds)
                    {
                        Debug.Log($"[NFTGate][ERC1155|SpecificToken] Checking tokenId {id}");
                        yield return StartCoroutine(CheckBalance1155(
                            cond.contractAddress, ownerHex, id,
                            result => unlocked |= result
                        ));
                        Debug.Log($"[NFTGate][ERC1155|SpecificToken] tokenId {id} owns = {unlocked}");
                        if (unlocked) break;
                    }
                }
            }
            else // ERC-721
            {
                if (cond.unlockMode == GateCondition.UnlockMode.AnyToken)
                {
                    Debug.Log("[NFTGate][ERC721|AnyToken] Checking balanceOf > 0");
                    yield return StartCoroutine(CheckBalance721(
                        cond.contractAddress, ownerHex,
                        result => unlocked = result
                    ));
                    Debug.Log($"[NFTGate][ERC721|AnyToken] owns = {unlocked}");
                }
                else
                {
                    foreach (var id in cond.tokenIds)
                    {
                        Debug.Log($"[NFTGate][ERC721|SpecificToken] Checking ownerOf {id}");
                        yield return StartCoroutine(CheckOwnerOf721(
                            cond.contractAddress, wallet, id,
                            result => unlocked |= result
                        ));
                        Debug.Log($"[NFTGate][ERC721|SpecificToken] tokenId {id} owns = {unlocked}");
                        if (unlocked) break;
                    }
                }
            }

            foreach (var btn in cond.targetButtons)
            {
                if (btn != null)
                {
                    btn.interactable = unlocked;
                    Debug.Log($"[NFTGate][Result] Button '{btn.name}' set interactable={unlocked}");
                }
            }
        }

        Debug.Log("[NFTGate][CheckAll] Verification complete");
    }

    IEnumerator CheckBalance1155(string contract, string ownerHex, string tokenId, Action<bool> cb)
    {
        string idHex = BigInteger.Parse(tokenId).ToString("X").PadLeft(64, '0');
        string data  = SEL_ERC1155_BALANCE + ownerHex + idHex;
        Debug.Log($"[NFTGate][RPC] ERC1155 balanceOf data={data}");
        yield return CallRpc(contract, data, cb, res =>
        {
            Debug.Log($"[NFTGate][Parse] ERC1155 res={res}");
            var bal = BigInteger.Parse(res.Substring(2), System.Globalization.NumberStyles.HexNumber);
            return bal > 0;
        });
    }

    IEnumerator CheckBalance721(string contract, string ownerHex, Action<bool> cb)
    {
        string data = SEL_ERC721_BALANCE + ownerHex + new string('0', 64);
        Debug.Log($"[NFTGate][RPC] ERC721 balanceOf data={data}");
        yield return CallRpc(contract, data, cb, res =>
        {
            Debug.Log($"[NFTGate][Parse] ERC721 res={res}");
            var bal = BigInteger.Parse(res.Substring(2), System.Globalization.NumberStyles.HexNumber);
            return bal > 0;
        });
    }

    IEnumerator CheckOwnerOf721(string contract, string wallet, string tokenId, Action<bool> cb)
    {
        string idHex = BigInteger.Parse(tokenId).ToString("X").PadLeft(64, '0');
        string data  = SEL_ERC721_OWNER + idHex;
        Debug.Log($"[NFTGate][RPC] ownerOf data={data}");
        yield return CallRpc(contract, data, cb, res =>
        {
            Debug.Log($"[NFTGate][Parse] ownerOf res={res}");
            string owner = "0x" + res.Substring(res.Length - 40);
            return string.Equals(owner, wallet, StringComparison.OrdinalIgnoreCase);
        });
    }

    IEnumerator CheckAnyTokenERC1155(string contract, string wallet, Action<bool> cb)
    {
        Debug.Log("[NFTGate][Logs] Fetching latest blockNumber");
        BigInteger latest = 0;
        yield return StartCoroutine(CallRpcRaw(new JObject{
            ["jsonrpc"]="2.0", ["method"]="eth_blockNumber", ["params"]=new JArray(), ["id"]=1
        }, json =>
        {
            latest = BigInteger.Parse(
                JObject.Parse(json)["result"].Value<string>().Substring(2),
                System.Globalization.NumberStyles.HexNumber
            );
            Debug.Log($"[NFTGate][Logs] latest block = {latest}");
        }));

        string topicTo = "0x" + wallet.Substring(2).PadLeft(64, '0');
        BigInteger chunk = 100, start = 0;
        bool found = false;
        while (start <= latest && !found)
        {
            BigInteger end = BigInteger.Min(start + chunk - 1, latest);
            Debug.Log($"[NFTGate][Logs] Scanning blocks {start} to {end}");
            var filter = new JObject{
                ["address"]   = contract,
                ["fromBlock"] = "0x" + start.ToString("X"),
                ["toBlock"]   = "0x" + end  .ToString("X"),
                ["topics"]    = new JArray(SIG_ERC1155_LOG, null, null, topicTo)
            };
            yield return StartCoroutine(CallRpcRaw(new JObject{
                ["jsonrpc"]="2.0", ["method"]="eth_getLogs",
                ["params"]=new JArray(filter), ["id"]=1
            }, json =>
            {
                var logs = JObject.Parse(json)["result"] as JArray;
                Debug.Log($"[NFTGate][Logs] Retrieved {logs?.Count ?? 0} logs");
                if (logs != null && logs.Count > 0) found = true;
            }));
            start += chunk;
        }
        Debug.Log($"[NFTGate][Logs] AnyToken result = {found}");
        cb(found);
    }

    IEnumerator CallRpc(string contract, string data, Action<bool> cb, Func<string, bool> parse)
    {
        var payload = new JObject(
            new JProperty("jsonrpc", "2.0"),
            new JProperty("method", "eth_call"),
            new JProperty("params", new JArray(
                new JObject(new JProperty("to", contract), new JProperty("data", data)),
                "latest"
            )),
            new JProperty("id", 1)
        );
        yield return CallRpcRaw(payload, json =>
        {
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError("[NFTGate][RPC] Empty response");
                cb(false);
                return;
            }
            string res = JObject.Parse(json)["result"].Value<string>();
            cb(parse(res));
        });
    }

    IEnumerator CallRpcRaw(JObject payload, Action<string> onResult)
    {
        Debug.Log($"[NFTGate][RPC] Sending payload: {payload}");
        using var uwr = new UnityWebRequest(rpcUrl, "POST")
        {
            uploadHandler   = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(payload.ToString())),
            downloadHandler = new DownloadHandlerBuffer()
        };
        uwr.SetRequestHeader("Content-Type", "application/json");
        yield return uwr.SendWebRequest();
        if (uwr.result != UnityWebRequest.Result.Success)
            Debug.LogError($"[NFTGate][RPC] Error: {uwr.error}");
        onResult(uwr.downloadHandler.text);
    }
}
