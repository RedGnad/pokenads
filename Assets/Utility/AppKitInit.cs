using Reown.AppKit.Unity;
using Reown.AppKit.Unity.Model;
using Reown.Core.Common.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityLogger = Reown.Sign.Unity.UnityLogger;

namespace Sample
{
    public class AppKitInit : MonoBehaviour
    {
        [Header("Scene Management")]
        [SerializeField] private bool   shouldSwitchScene = false;
        [SerializeField] private string targetSceneName   = "";

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
                projectId                     = "27f51a8cead380193aaf687f55e3d4af",
                metadata                      = new Metadata(
                    "Pokenads",
                    "AppKit Unity Sample - Monad Testnet",
                    "https://reown.com",
                    "https://raw.githubusercontent.com/reown-com/reown-dotnet/main/media/appkit-icon.png",
                    new RedirectData { Native = "appkit-sample-unity://" }
                ),
                customWallets                 = GetCustomWallets(),  // NE REMPLACE QUE SUR ANDROID
                connectViewWalletsCountMobile = 5,
                supportedChains               = new[] { monadTestnet },
                socials                       = new[]
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

                if (shouldSwitchScene && Application.CanStreamedLevelBeLoaded(targetSceneName))
                    SceneManager.LoadScene(targetSceneName);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[AppKitInit] Init failed: " + ex);
            }
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