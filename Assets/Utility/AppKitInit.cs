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
        // Option pour changer de scène ou non
        [Header("Scene Management")]
        [SerializeField] private bool shouldSwitchScene = false;
        [SerializeField] private string targetSceneName = "";

        // Ces variables sont conservées mais ignorées si shouldSwitchScene = false
        [Header("Legacy Options (Ignore)")]
        [SerializeField] private string _menuScene = "Menu";
        [SerializeField] private bool loadSampleScreenDirectly = false;
        [SerializeField] private string sampleSceneName = "SampleScreen";

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
            
            var appKitConfig = new AppKitConfig
            {
                projectId = "27f51a8cead380193aaf687f55e3d4af",
                metadata = new Metadata(
                    "Pokenads",
                    "AppKit Unity Sample - Monad Testnet",
                    "https://reown.com",
                    "https://raw.githubusercontent.com/reown-com/reown-dotnet/main/media/appkit-icon.png",
                    new RedirectData
                    {
                        Native = "appkit-sample-unity://"
                    }
                ),
                customWallets = GetCustomWallets(),
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

            Debug.Log("[AppKit Init] Initializing AppKit with Monad Testnet...");

            try
            {
                await AppKit.InitializeAsync(appKitConfig);
                Debug.Log("[AppKit Init] AppKit initialized successfully!");
                
                // Ne changez de scène que si l'option est activée et qu'un nom de scène valide est fourni
                if (shouldSwitchScene && !string.IsNullOrEmpty(targetSceneName))
                {
                    if (Application.CanStreamedLevelBeLoaded(targetSceneName))
                    {
                        Debug.Log($"[AppKit Init] Switching to scene: {targetSceneName}");
                        SceneManager.LoadScene(targetSceneName);
                    }
                    else
                    {
                        Debug.LogWarning($"[AppKit Init] Cannot load scene '{targetSceneName}'. It's not in build settings.");
                    }
                }
                else
                {
                    Debug.Log("[AppKit Init] Scene switching disabled. AppKit ready to use in current scene.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AppKit Init] Failed to initialize AppKit: {ex.Message}");
            }
        }

        /// <summary>
        ///     This method returns a list of Reown sample wallets on iOS and Android.
        ///     These wallets are used for testing and are not included in the default list of wallets returned by AppKit's REST API.
        ///     On other platforms, this method returns null, so only the default list of wallets is used.
        /// </summary>
        private Wallet[] GetCustomWallets()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return new[]
            {
                new Wallet
                {
                    Name = "Swift Wallet",
                    ImageUrl = "https://github.com/reown-com/reown-dotnet/blob/develop/media/wallet-swift.png?raw=true",
                    MobileLink = "walletapp://"
                },
                new Wallet
                {
                    Name = "React Native Wallet",
                    ImageUrl = "https://github.com/reown-com/reown-dotnet/blob/develop/media/wallet-rn.png?raw=true",
                    MobileLink = "rn-web3wallet://"
                },
                new Wallet
                {
                    Name = "Flutter Wallet Prod",
                    ImageUrl = "https://github.com/reown-com/reown-dotnet/blob/develop/media/wallet-flutter.png?raw=true",
                    MobileLink = "wcflutterwallet://"
                }
            };
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
            return new[]
            {
                new Wallet
                {
                    Name = "Phantom",
                    ImageUrl = "https://raw.githubusercontent.com/WalletConnect/walletconnect-assets/master/Icon/Platform/HaHa/Icon.png", 
                    MobileLink = "phantom://"
                },
                new Wallet
                {
                    Name = "Rabby Wallet",
                    ImageUrl = "https://raw.githubusercontent.com/WalletConnect/walletconnect-assets/master/Icon/Platform/Rabby/Icon.svg",
                    MobileLink = "rabby://"
                },
                new Wallet
                {
                    Name = "HaHa Wallet",
                    ImageUrl = "https://raw.githubusercontent.com/WalletConnect/walletconnect-assets/master/Icon/Platform/HaHa/Icon.png", 
                    MobileLink = "haha://"
                }
            };
#endif
            return null;
        }
    }
}