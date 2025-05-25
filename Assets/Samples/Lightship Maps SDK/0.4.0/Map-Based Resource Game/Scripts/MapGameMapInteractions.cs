using System;
using Niantic.Lightship.Maps.Core.Coordinates;
using Niantic.Lightship.Maps.MapLayers.Components;
using UnityEngine;

namespace Niantic.Lightship.Maps.Samples.GameSample
{
    internal class MapGameMapInteractions : MonoBehaviour
    {
        [SerializeField]
        private Camera _mapCamera;

        [SerializeField]
        private LightshipMapView _lightshipMapView;

        [SerializeField]
        private FloatingText.FloatingText _floatingTextPrefab;

        [SerializeField]
        private LayerGameObjectPlacement _sawmillSpawner;

        [SerializeField]
        private LayerGameObjectPlacement _stoneMasonSpawner;

        [SerializeField]
        private LayerGameObjectPlacement _strongholdSpawner;

        // Nouvelles variables pour le son de distance
        [Header("Distance Sound")]
        [SerializeField] private AudioClip tooFarSound;
        private AudioSource audioSource;

        // Nouvelles variables pour vérifier l'existence du bloqueur modal
        private bool isWalletModalOpen = false;
        private float lastCheckTime = 0f;
        private const float CHECK_INTERVAL = 0.5f; // Vérifier toutes les 0.5 secondes

        private MapGameState.StructureType _placingStructureType;
        private bool _placingStructure;

        private void Awake()
        {
            // Initialiser l'AudioSource
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        public void StartPlacingStructure(MapGameState.StructureType structureType)
        {
            _placingStructureType = structureType;
            _placingStructure = true;
        }

        private void Update()
        {
            // Vérifier périodiquement si l'objet bloqueur existe
            if (Time.time > lastCheckTime + CHECK_INTERVAL)
            {
                lastCheckTime = Time.time;
                isWalletModalOpen = GameObject.Find("WalletModalBlocker") != null;
                
                if (isWalletModalOpen)
                {
                    Debug.Log("[MapGameMapInteractions] Détection du bloqueur de modal wallet");
                }
            }
            
            // NOUVEAU: Si l'inventaire est ouvert OU le modal wallet est ouvert, ignorer les entrées
            if (InventoryUI.IsInventoryOpen || isWalletModalOpen)
            {
                return;
            }
            
            var touchPosition = Vector3.zero;
            bool touchDetected = false;

            if (Input.touchCount == 1)
            {
                if (Input.touches[0].phase == TouchPhase.Ended)
                {
                    touchPosition = Input.touches[0].position;
                    touchDetected = true;
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                touchPosition = Input.mousePosition;
                touchDetected = true;
            }

            if (touchDetected)
            {
                if (_placingStructure)
                {
                    PlaceStructure(touchPosition);
                }
                else
                {
                    CheckForInteractableTouch(touchPosition);
                }
            }
        }

        private LatLng ScreenPointToLatLong(Vector3 screenPosition)
        {
            var clickRay = _mapCamera.ScreenPointToRay(screenPosition);
            var pointOnMap = clickRay.origin + clickRay.direction * (-clickRay.origin.y / clickRay.direction.y);
            LatLng latLng = _lightshipMapView.SceneToLatLng(pointOnMap);
            return latLng;
        }

        private void PlaceStructure(Vector3 touchPosition)
        {
            // NOUVEAU: Vérifier si l'inventaire est ouvert OU le modal wallet est ouvert
            if (InventoryUI.IsInventoryOpen || isWalletModalOpen)
            {
                Debug.Log("[MapGameMapInteractions] Placement de structure ignoré - UI modale ouverte");
                return;
            }
            
            var structureLatLng = ScreenPointToLatLong(touchPosition);
            var cameraForward = _mapCamera.transform.forward;
            var forward = new Vector3(cameraForward.x, 0f, cameraForward.z).normalized;
            var rotation = Quaternion.LookRotation(forward);

            switch (_placingStructureType)
            {
                case MapGameState.StructureType.Sawmill:
                    _sawmillSpawner.PlaceInstance(structureLatLng, rotation);
                    break;
                case MapGameState.StructureType.StoneMason:
                    _stoneMasonSpawner.PlaceInstance(structureLatLng, rotation);
                    MapGameState.Instance.EnableResourceProduction(MapGameState.ResourceType.Stone, true);
                    break;
                case MapGameState.StructureType.Stronghold:
                    _strongholdSpawner.PlaceInstance(structureLatLng, rotation);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_placingStructureType));
            }

            MapGameState.Instance.StructureBuilt(structureLatLng, _placingStructureType);
            _placingStructure = false;
        }

        private void CheckForInteractableTouch(Vector3 touchPosition)
        {
            // NOUVEAU: Vérifier si l'inventaire est ouvert OU le modal wallet est ouvert
            if (InventoryUI.IsInventoryOpen || isWalletModalOpen)
            {
                Debug.Log("[MapGameMapInteractions] Interaction ignorée - UI modale ouverte");
                return;
            }
            
            // Obtenir le rayon à partir du point de l'écran
            var touchRay = _mapCamera.ScreenPointToRay(touchPosition);

            if (!Physics.Raycast(touchRay, out var hitInfo))
            {
                return;
            }

            var hitResourceItem = hitInfo.collider.GetComponent<MapGameResourceFeature>();
            if (hitResourceItem == null)
            {
                return;
            }

            if (!hitResourceItem.ResourcesAvailable)
            {
                return;
            }

            int amount = hitResourceItem.GainResources();
            MapGameState.Instance.AddResource(hitResourceItem.ResourceType, amount);

            if (amount == 0)
            {
                // Le joueur est trop loin - jouer le son
                if (audioSource != null && tooFarSound != null)
                {
                    audioSource.PlayOneShot(tooFarSound);
                    Debug.Log("Ressource trop éloignée - Son joué");
                }
                
                // Afficher le texte flottant existant
                var floatingTextPosition = hitInfo.point + Vector3.up * 20.0f;
                var forward = floatingTextPosition - _mapCamera.transform.position;
                var rotation = Quaternion.LookRotation(forward, Vector3.up);
                var floatText = Instantiate(_floatingTextPrefab, floatingTextPosition, rotation);
                floatText.SetText("Come closer");
            }
        }
    }
}