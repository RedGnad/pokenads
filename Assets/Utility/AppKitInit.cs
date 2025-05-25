using Reown.AppKit.Unity;
using Reown.AppKit.Unity.Model;
using Reown.Core.Common.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityLogger = Reown.Sign.Unity.UnityLogger;
using System.Collections.Generic;
using System.Collections;

namespace Sample
{
    public class AppKitInit : MonoBehaviour
    {
        [Header("Scene Management")]
        [SerializeField] private bool shouldSwitchScene = false;
        [SerializeField] private string targetSceneName = "";

        [Header("Interaction Management")]
        [Tooltip("Désactiver les interactions 3D quand un modal AppKit est ouvert")]
        [SerializeField] private bool disableInteractionsOnModal = true;
        
        [Tooltip("Types de scripts d'interaction à désactiver")]
        [SerializeField] private string[] interactionScriptNames = {
            "FeatureInteraction",
            "MapGameMapInteractions"
        };
        
        [Tooltip("Intervalle de vérification du modal (secondes)")]
        [SerializeField] private float checkInterval = 0.2f;
        
        // Liste des composants désactivés
        private List<MonoBehaviour> disabledComponents = new List<MonoBehaviour>();
        
        // État du modal
        private bool isModalActive = false;
        
        // Pour la vérification périodique
        private Coroutine modalCheckCoroutine;

        private async void Start()
        {
            ReownLogger.Instance = new UnityLogger();

            var monadTestnet = new Chain(
                ChainConstants.Namespaces.Evm,
                chainReference: "10143",
                name: "Monad Testnet",
                nativeCurrency: new Currency("Monad", "MON", 18),
                blockExplorer: new BlockExplorer("Monad Explorer", "https://explorer.testnet.monad.xyz"),
                rpcUrl: "https://rpc.testnet.monad.xyz/",
                isTestnet: true,
                imageUrl: "https://monad.xyz/logo.svg"
            );

            var cfg = new AppKitConfig
            {
                projectId = "27f51a8cead380193aaf687f55e3d4af",
                metadata = new Metadata(
                    "Pokenads",
                    "AppKit Unity Sample - Monad Testnet",
                    "https://pokenads-c58e5.web.app",
                    "https://raw.githubusercontent.com/RedGnad/pokenads/master/pokenads-logo8.png",
                    new RedirectData { Native = "appkit-sample-unity://" }
                ),
                customWallets = GetCustomWallets(),  // NE REMPLACE QUE SUR ANDROID
                connectViewWalletsCountMobile = 5,
                supportedChains = new[] { monadTestnet },
                socials = new[]
                {
                    SocialLogin.Google,
                    SocialLogin.X,
                    SocialLogin.Discord,
                    SocialLogin.Apple,
                    SocialLogin.GitHub
                }
            };

            try
            {
                Debug.Log("[AppKitInit] Initializing…");
                await AppKit.InitializeAsync(cfg);
                Debug.Log("[AppKitInit] Initialized!");

                // S'abonner aux événements après l'initialisation
                if (disableInteractionsOnModal)
                {
                    // S'abonner aux événements wallet qui sont souvent liés aux modaux
                    AppKit.AccountConnected += OnWalletEvent;
                    AppKit.AccountDisconnected += OnWalletEvent;
                    
                    // Démarrer la vérification périodique du modal
                    StartModalCheck();
                }

                if (shouldSwitchScene && Application.CanStreamedLevelBeLoaded(targetSceneName))
                    SceneManager.LoadScene(targetSceneName);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[AppKitInit] Init failed: " + ex);
            }
        }
        
        private void OnWalletEvent(object sender, System.EventArgs e)
        {
            // Après une connexion ou déconnexion, le modal est généralement fermé
            isModalActive = false;
            EnableAllInteractions();
        }
        
        private void StartModalCheck()
        {
            if (modalCheckCoroutine != null)
            {
                StopCoroutine(modalCheckCoroutine);
            }
            
            modalCheckCoroutine = StartCoroutine(CheckModalRoutine());
        }
        
        private IEnumerator CheckModalRoutine()
        {
            while (true)
            {
                bool modalDetected = IsModalVisible();
                
                // Si l'état a changé
                if (modalDetected != isModalActive)
                {
                    isModalActive = modalDetected;
                    
                    if (modalDetected)
                    {
                        DisableAllInteractions();
                    }
                    else
                    {
                        EnableAllInteractions();
                    }
                }
                
                yield return new WaitForSeconds(checkInterval);
            }
        }
        
        private bool IsModalVisible()
        {
            // Essayer de détecter le modal via les GameObject typiques d'AppKit
            GameObject modalContainer = GameObject.Find("AppKit_ModalContainer");
            if (modalContainer != null && modalContainer.activeInHierarchy)
                return true;
                
            // Chercher des Canvas qui pourraient être des modaux AppKit
            Canvas[] canvases = FindObjectsOfType<Canvas>();
            foreach (Canvas canvas in canvases)
            {
                string name = canvas.name.ToLower();
                if ((name.Contains("modal") || name.Contains("wallet") || name.Contains("connect")) 
                    && canvas.gameObject.activeInHierarchy)
                {
                    return true;
                }
            }
            
            return false;
        }
        
        private void DisableAllInteractions()
        {
            // Réinitialiser la liste
            disabledComponents.Clear();
            
            // Désactiver tous les scripts d'interaction connus
            MonoBehaviour[] allBehaviours = FindObjectsOfType<MonoBehaviour>();
            foreach (var script in allBehaviours)
            {
                if (script == null) continue;
                
                string typeName = script.GetType().Name;
                
                foreach (string scriptName in interactionScriptNames)
                {
                    if (typeName == scriptName && script.enabled)
                    {
                        script.enabled = false;
                        disabledComponents.Add(script);
                        Debug.Log($"[AppKitInit] Désactivation de {script.name} ({typeName})");
                        break;
                    }
                }
            }
            
            Debug.Log($"[AppKitInit] {disabledComponents.Count} scripts d'interaction désactivés");
        }
        
        private void EnableAllInteractions()
        {
            foreach (var script in disabledComponents)
            {
                if (script != null)
                {
                    script.enabled = true;
                    Debug.Log($"[AppKitInit] Réactivation de {script.name} ({script.GetType().Name})");
                }
            }
            
            disabledComponents.Clear();
            Debug.Log("[AppKitInit] Tous les scripts d'interaction réactivés");
        }
        
        private void OnDestroy()
        {
            // Se désabonner des événements
            if (AppKit.IsInitialized)
            {
                AppKit.AccountConnected -= OnWalletEvent;
                AppKit.AccountDisconnected -= OnWalletEvent;
            }
            
            // Arrêter la coroutine
            if (modalCheckCoroutine != null)
            {
                StopCoroutine(modalCheckCoroutine);
                modalCheckCoroutine = null;
            }
            
            // S'assurer que tout est réactivé
            EnableAllInteractions();
        }

        private Wallet[] GetCustomWallets()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return new[]
            {
                new Wallet
                {
                    Name       = "Phantom",
                    ImageUrl   = "https://raw.githubusercontent.com/WalletConnect/walletconnect-assets/master/Icon/Platform/HaHa/Icon.png",
                    MobileLink = "phantom://wc"
                },
                new Wallet
                {
                    Name       = "Backpack",
                    ImageUrl   = "https://raw.githubusercontent.com/WalletConnect/walletconnect-assets/master/Icon/Platform/Backpack/Icon.png",
                    MobileLink = "backpack://"
                },
                new Wallet
                {
                    Name       = "HaHa Wallet",
                    ImageUrl   = "https://raw.githubusercontent.com/WalletConnect/walletconnect-assets/master/Icon/Platform/HaHa/Icon.png",
                    MobileLink = "haha://"
                },
                new Wallet
                {
                    Name       = "Rabby (soon)",
                    ImageUrl   = "https://raw.githubusercontent.com/WalletConnect/walletconnect-assets/master/Icon/Platform/MetaMask/Icon.png",
                    MobileLink = "rabby://"
                }
            };
#else
            // Sur iOS ou Éditeur, on utilise la liste par défaut AppKit
            return null;
#endif
        }
    }
}