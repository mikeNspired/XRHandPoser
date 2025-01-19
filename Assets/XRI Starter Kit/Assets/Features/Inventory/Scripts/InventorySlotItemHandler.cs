using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace MikeNspired.UnityXRHandPoser
{
    /// <summary>
    /// Handles all item logic: spawning a starting item, swapping, disabling in-hand,
    /// and mesh clone creation/scaling. No animation coroutines for UI icons.
    /// </summary>
    public class InventorySlotItemHandler : MonoBehaviour
    {
        [Header("Visual Slot Displays")]
        [SerializeField] private GameObject slotDisplayWhenContainsItem;
        [SerializeField] private GameObject slotDisplayToAddItem;

        [Header("Transforms & Colliders")]
        [SerializeField] private Transform itemModelHolder;
        [SerializeField] private Transform backImagesThatRotate;
        [SerializeField] private BoxCollider inventorySize;

        [Header("Audio")]
        [SerializeField] private AudioSource grabAudio;
        [SerializeField] private AudioSource releaseAudio;

        public GameObject SlotDisplayWhenContainsItem => slotDisplayWhenContainsItem;
        public GameObject SlotDisplayToAddItem => slotDisplayToAddItem;

        public XRBaseInteractable CurrentSlotItem { get; private set; }

        private TransformStruct itemStartingTransform;
        private Transform boundCenterTransform, itemSlotMeshClone;
        private Vector3 goalSizeToFitInSlot;

        public float AnimationLengthItemToSlot = 0.15f;
        private Coroutine animateItemToSlotCoroutine;
        private XRInteractionManager interactionManager;
        private bool isBusy;

        public void Setup(XRBaseInteractable prefab)
        {
            interactionManager = FindFirstObjectByType<XRInteractionManager>();

            if (!boundCenterTransform)
            {
                boundCenterTransform = new GameObject("Bound Center Transform").transform;
                boundCenterTransform.SetParent(itemModelHolder);
            }
            
            //Create starting slot item
            if (prefab)
            {
                CurrentSlotItem = Instantiate(prefab);
                CurrentSlotItem.transform.SetParent(transform);        
                CurrentSlotItem.transform.localPosition = Vector3.zero;
                CurrentSlotItem.transform.localEulerAngles = Vector3.zero;

                SetupNewMeshClone(CurrentSlotItem);
                CurrentSlotItem.gameObject.SetActive(false);
                SnapItemToSlot();
            }

            gameObject.SetActive(false);
        }
        
        public void SetSlotDisplayInstant()
        {
            if (CurrentSlotItem)
            {
                if (SlotDisplayWhenContainsItem)
                    SlotDisplayWhenContainsItem.SetActive(true);
                if (SlotDisplayToAddItem)
                    SlotDisplayToAddItem.SetActive(false);
            }
            else
            {
                if (SlotDisplayWhenContainsItem)
                    SlotDisplayWhenContainsItem.SetActive(false);
                if (SlotDisplayToAddItem)
                    SlotDisplayToAddItem.SetActive(true);
            }
        }
        
        private IEnumerator AnimateIcon()
        {
            isBusy = true;
            if (CurrentSlotItem) //If has item show item
            {
                //if (animateItemToSlotCoroutine != null) StopCoroutine(animateItemToSlotCoroutine);
                slotDisplayWhenContainsItem.gameObject.SetActive(true);
                yield return new WaitForSeconds(.5f / 2);
                slotDisplayToAddItem.gameObject.SetActive(false);
            }
            else //Show add item display
            {
                slotDisplayToAddItem.gameObject.SetActive(true);
                slotDisplayWhenContainsItem.gameObject.SetActive(false);
            }
            isBusy = false;

            //Better user experience  after waiting to enable collider after some visuals start appearing
            //collider.enabled = true;

        }
        
        public IEnumerator AnimateMeshModelOpenOrClose(bool toOne, float duration)
        {
            float timer = 0f;
            Vector3 initialScale = toOne ? Vector3.zero : Vector3.one;
            Vector3 targetScale = toOne ? Vector3.one : Vector3.zero;

            while (timer < duration)
            {
                float t = Mathf.Clamp01(timer / duration);
                itemModelHolder.localScale = Vector3.Lerp(initialScale, targetScale, t);

                yield return null;
                timer += Time.deltaTime;
            }

            // Ensure the final scale is exactly set to the target
            itemModelHolder.localScale = targetScale;
        }

        // ─────────────────────────────────────────────────────────────────
        //  2) Interaction
        // ─────────────────────────────────────────────────────────────────

        public void InteractWithSlot(XRBaseInteractor controller)
        {
            if (!controller) return;            
            if (isBusy) return;
            
            if (animateItemToSlotCoroutine != null)
                StopCoroutine(animateItemToSlotCoroutine);

            //If hand has item that cannot be inventoried, return
            var itemInHand = GetItemInHand(controller);
            if (itemInHand && !CanInventory(itemInHand)) return;
            
            if(itemInHand)
                AddItemToSlot(controller);

            if (CurrentSlotItem) 
                RetrieveItemFromSlot(controller, !itemInHand);

            if (itemInHand)
                CurrentSlotItem = itemInHand;

            StartCoroutine(AnimateIcon());
        }

        static bool CanInventory(XRBaseInteractable item)
        {
            var itemData = item.GetComponent<InteractableItemData>();
            return itemData == null || itemData.canInventory;
        }

        static XRBaseInteractable GetItemInHand(XRBaseInteractor controller)
        {
            if (!controller.hasSelection) return null;
            if (controller.interactablesSelected.Count == 0) return null;
            return controller.interactablesSelected[0] as XRBaseInteractable;
        }

        void AddItemToSlot(XRBaseInteractor controller)
        {
            var itemHandIsHolding = GetItemInHand(controller);
            if (!itemHandIsHolding) return;

            releaseAudio?.Play();
            ReleaseItemFromHand(controller, itemHandIsHolding);

            // Move item transform under this slot, then disable it
            itemHandIsHolding.transform.SetParent(transform);
  
            var grabDisable = itemHandIsHolding.GetComponent<OnGrabEnableDisable>();
            grabDisable?.EnableAll();
            
            if (CurrentSlotItem)
                CurrentSlotItem.transform.localPosition = Vector3.zero;
            
            SetupNewMeshClone(itemHandIsHolding);   
            itemHandIsHolding.gameObject.SetActive(false);

            animateItemToSlotCoroutine = StartCoroutine(AnimateItemToSlot());
        }

        void RetrieveItemFromSlot(XRBaseInteractor controller, bool destroyItemMesh)
        {
            if (!CurrentSlotItem) return;
            if (itemSlotMeshClone && destroyItemMesh)
                Destroy(itemSlotMeshClone.gameObject);
            
            CurrentSlotItem.gameObject.SetActive(true);
            CurrentSlotItem.transform.SetParent(null);
            GrabNewItem(controller, CurrentSlotItem);
            grabAudio?.Play();

            CurrentSlotItem = null;
        }

        void ReleaseItemFromHand(XRBaseInteractor interactor, XRBaseInteractable interactable)
        {
            if (interactionManager)
                interactionManager.SelectExit((IXRSelectInteractor) interactor,  interactable);
        }

        void GrabNewItem(XRBaseInteractor interactor, XRBaseInteractable interactable)
        {
            if (interactionManager)
                interactionManager.SelectEnter((IXRSelectInteractor) interactor, interactable);
        }

        IEnumerator AnimateItemToSlot()
        {
            isBusy = true;
            float timer = 0f;

            while (timer < AnimationLengthItemToSlot + Time.deltaTime)
            {
                float t = timer / AnimationLengthItemToSlot;
                boundCenterTransform.localPosition = Vector3.Lerp(itemStartingTransform.position, Vector3.zero, t);
                boundCenterTransform.localRotation = Quaternion.Lerp(itemStartingTransform.rotation, Quaternion.Euler(0, 90, 0), t);
                boundCenterTransform.localScale    = Vector3.Lerp(itemStartingTransform.scale, goalSizeToFitInSlot, t);
                yield return null;
                timer += Time.deltaTime;
            }
            
            isBusy = false;
        }
        
        private void SnapItemToSlot()
        {
            // Set the final state immediately
            boundCenterTransform.localPosition = Vector3.zero;
            boundCenterTransform.localScale = goalSizeToFitInSlot;
            boundCenterTransform.localRotation = Quaternion.Euler(0, 90, 0);
        }
        
        // ─────────────────────────────────────────────────────────────────
        //  3) Creating Item Mesh Clone
        // ─────────────────────────────────────────────────────────────────

        void SetupNewMeshClone(XRBaseInteractable itemToClone)
        {
            if (itemSlotMeshClone)
                Destroy(itemSlotMeshClone.gameObject);
            
            itemSlotMeshClone = GameObjectCloner.DuplicateAndStrip(itemToClone.gameObject).transform;
            itemSlotMeshClone.SetPositionAndRotation(
                itemToClone.transform.position,
                itemToClone.transform.rotation
            );

            //Get center of meshs as boundCenterTransform
            Bounds bounds = GetBoundsOfAllMeshes(itemSlotMeshClone.transform);
            boundCenterTransform.localScale = Vector3.one;
            boundCenterTransform.rotation = itemToClone.transform.rotation;
            boundCenterTransform.position = bounds.center;
            
            itemSlotMeshClone.SetParent(boundCenterTransform);
            
            //Set current transform to animate item to the slot
            itemStartingTransform.SetTransformStruct(
                boundCenterTransform.localPosition,
                boundCenterTransform.localRotation,
                boundCenterTransform.localScale
            );

            //Rotate to face all items to right
            boundCenterTransform.localEulerAngles = new Vector3(0, 90, 0);
            
            //Shrink item to find inside inventorySize
            if (inventorySize) inventorySize.enabled = true;
            Vector3 parentSize = inventorySize.bounds.size;
            while (bounds.size.x > parentSize.x || bounds.size.y > parentSize.y || bounds.size.z > parentSize.z)
            {
                bounds = GetBoundsOfAllMeshes(itemSlotMeshClone.transform);
                boundCenterTransform.localScale *= 0.9f;
            }
            if (inventorySize) inventorySize.enabled = false;
            
            goalSizeToFitInSlot = boundCenterTransform.localScale;
        }
      

        /// <summary>
        /// Recursively clones a hierarchy so that only transforms,
        /// MeshFilters, and MeshRenderers are copied.
        /// </summary>
        /// <param name="original">Original Transform to clone from.</param>
        /// <param name="parent">Where to parent the cloned Transform.</param>
        /// <returns>The cloned Transform's root.</returns>
        private static Transform CloneMeshHierarchy(Transform original, Transform parent = null)
        {
            // Create a new GameObject for this node
            GameObject cloneGO = new GameObject(original.name);
            cloneGO.transform.SetParent(parent, worldPositionStays: false);

            // Match local transform (IMPORTANT to keep the same shape/position)
            cloneGO.transform.localPosition = original.localPosition;
            cloneGO.transform.localRotation = original.localRotation;
            cloneGO.transform.localScale = original.localScale;

            // Handle MeshFilter and MeshRenderer
            MeshFilter originalMeshFilter = original.GetComponent<MeshFilter>();
            MeshRenderer originalMeshRenderer = original.GetComponent<MeshRenderer>();

            if (originalMeshFilter && originalMeshRenderer.enabled)
            {
                MeshFilter newMF = cloneGO.AddComponent<MeshFilter>();
                newMF.sharedMesh = originalMeshFilter.sharedMesh;

                MeshRenderer newMR = cloneGO.AddComponent<MeshRenderer>();
                newMR.sharedMaterials = originalMeshRenderer.sharedMaterials;
                newMR.shadowCastingMode = originalMeshRenderer.shadowCastingMode;
                newMR.receiveShadows = originalMeshRenderer.receiveShadows;
            }

            // Handle SkinnedMeshRenderer
            SkinnedMeshRenderer originalSkinnedRenderer = original.GetComponent<SkinnedMeshRenderer>();
            if (originalSkinnedRenderer && originalSkinnedRenderer.enabled)
            {
                SkinnedMeshRenderer newSMR = cloneGO.AddComponent<SkinnedMeshRenderer>();
                newSMR.sharedMesh = originalSkinnedRenderer.sharedMesh;
                newSMR.sharedMaterials = originalSkinnedRenderer.sharedMaterials;

                // Copy additional SkinnedMeshRenderer properties
                newSMR.shadowCastingMode = originalSkinnedRenderer.shadowCastingMode;
                newSMR.receiveShadows = originalSkinnedRenderer.receiveShadows;
                newSMR.quality = originalSkinnedRenderer.quality;
                newSMR.updateWhenOffscreen = originalSkinnedRenderer.updateWhenOffscreen;

                // Copy bones and root bone if needed
                newSMR.bones = originalSkinnedRenderer.bones;
                newSMR.rootBone = originalSkinnedRenderer.rootBone;
            }

            // Recursively clone children
            for (int i = 0; i < original.childCount; i++)
            {
                Transform child = original.GetChild(i);
                if (child == original || child == cloneGO.transform)
                    continue; // skip any weird cyclical references

                if (child.gameObject.activeInHierarchy)
                    CloneMeshHierarchy(child, cloneGO.transform);
            }


            return cloneGO.transform;
        }

        static Bounds GetBoundsOfAllMeshes(Transform item)
        {
            Bounds bounds = new Bounds();
            var rends = item.GetComponentsInChildren<Renderer>(true);
            foreach (var rend in rends)
            {
                if (rend.GetComponent<ParticleSystem>()) continue;
                if (bounds.extents == Vector3.zero) bounds = rend.bounds;
                else bounds.Encapsulate(rend.bounds);
            }
            return bounds;
        }
    }
}
