using System;
using System.Linq;
using HarmonyLib;
using Newtonsoft.Json.Serialization;
using RoR2.UI;
using UnityEngine.UI;

#pragma warning disable

namespace Survariants {
    public class SurvivorVariantManager {
        public static void Initialize() {
            On.RoR2.UI.CharacterSelectController.RebuildLocal += CharacterSelectController_RebuildLocal;
            On.RoR2.UI.CharacterSelectController.Awake += CharacterSelectController_Awake;
            On.RoR2.CharacterSelectBarController.EnforceValidChoice += CharacterSelectBarController_EnforceValidChoice;
            On.RoR2.UI.HGHeaderNavigationController.RebuildHeaders += ThisFuckingSucks;
        }

        private static void ThisFuckingSucks(On.RoR2.UI.HGHeaderNavigationController.orig_RebuildHeaders orig, HGHeaderNavigationController self)
        {
            HGHeaderNavigationController.Header header = self.headers.FirstOrDefault(x => x.headerName == "SurvivorVariant");

            bool enabled = header.headerName == "SurvivorVariant" ? header.headerButton.interactable : false;

            orig(self);

            if (header.headerName == "SurvivorVariant") {
                header.headerButton.interactable = enabled;
            }
        }

        private static void CharacterSelectBarController_EnforceValidChoice(On.RoR2.CharacterSelectBarController.orig_EnforceValidChoice orig, CharacterSelectBarController self)
        {
            SurvivorDef surv = self.GetLocalUserExistingSurvivorPreference();
            if (SurvivorVariantCatalog.SurvivorVariantReverseMap.ContainsKey(surv)) {
                return;
            }

            orig(self);
        }

        private static void CharacterSelectController_Awake(On.RoR2.UI.CharacterSelectController.orig_Awake orig, RoR2.UI.CharacterSelectController self)
        {
            orig(self);

            SurvivorVariantUIMenuController controller = self.AddComponent<SurvivorVariantUIMenuController>();

            GameObject SurvivorVariantButton = GameObject.Instantiate(self.primaryColorImages[0].gameObject, self.primaryColorImages[0].transform.parent);
            SurvivorVariantButton.GetComponent<LanguageTextMeshController>().token = "Variants";
            SurvivorVariantButton.GetComponent<HGButton>().interactable = false;
            var navController = SurvivorVariantButton.transform.parent.GetComponent<HGHeaderNavigationController>();
            HGHeaderNavigationController.Header header = new();
            header.headerButton = SurvivorVariantButton.GetComponent<HGButton>();
            header.headerName = "SurvivorVariant";
            header.tmpHeaderText = navController.headers[0].tmpHeaderText;
            self.primaryColorImages.AddItem(SurvivorVariantButton.GetComponent<Image>());
            controller.SurvivorVariantButton = SurvivorVariantButton.GetComponent<HGButton>();

            GameObject SurvivorVariantContainer = GameObject.Instantiate(self.skillStripContainer.gameObject, self.skillStripContainer.parent);
            SurvivorVariantContainer.name = "SurvivorVariantContainer";
            SurvivorVariantContainer.SetActive(false);
            controller.SurvivorVariantContainer = SurvivorVariantContainer.transform;
            controller.CSS = self;

            header.headerRoot = SurvivorVariantContainer;

            HG.ArrayUtils.ArrayAppend(ref navController.headers, in header);
        }

        private static void CharacterSelectController_RebuildLocal(On.RoR2.UI.CharacterSelectController.orig_RebuildLocal orig, RoR2.UI.CharacterSelectController self)
        {
            orig(self);

            SurvivorVariantUIMenuController controller = self.GetComponent<SurvivorVariantUIMenuController>();

            if (!self.currentSurvivorDef) {
                return;
            }

            bool ownsSurvivorVariant = OwnsValidSurvivorVariant(self.currentSurvivorDef);

            // Debug.Log("owns SurvivorVariant: " + ownsSurvivorVariant);

            controller.SurvivorVariantButton.interactable = ownsSurvivorVariant;

            SurvivorDef surv = self.currentSurvivorDef;

            controller.SurvivorVariantButton.GetComponent<Image>().color = surv.bodyPrefab.GetComponent<CharacterBody>().bodyColor;

            if (ownsSurvivorVariant) {
                controller.PopulateSurvivorVariantForSurvivor(surv);
            }
            else if (controller.active) {
                controller.HideSurvivorVariant();
            }
        }

        private static bool OwnsValidSurvivorVariant(SurvivorDef surv) {
            return SurvivorVariantCatalog.SurvivorVariantReverseMap.ContainsKey(surv) || SurvivorVariantCatalog.SurvivorVariantMap.ContainsKey(surv);
        }

        public class SurvivorVariantUIMenuController : MonoBehaviour {
            public HGButton SurvivorVariantButton;
            public Transform SurvivorVariantContainer;
            public GameObject SkillPrefab;
            public GameObject SkillFillerPrefab;
            public CharacterSelectController CSS;
            public UIElementAllocator<RectTransform> SurvivorVariantAllocator;
            public UIElementAllocator<RectTransform> SurvivorVariantFillerAllocator;
            public bool active = false;

            public void Start() {

                SkillPrefab = SurvivorVariantContainer.transform.Find("SkillStripPrefab").gameObject;
                SkillFillerPrefab = SurvivorVariantContainer.transform.Find("SkillStripFillerPrefab").gameObject;

                SkillFillerPrefab.GetComponent<Image>().enabled = false;

                SurvivorVariantButton.onClick.RemoveAllListeners();
                SurvivorVariantButton.onClick.AddListener(DisplaySurvivorVariant);

                GameObject highlight = SkillPrefab.transform.Find("Inner/HoverHighlight").gameObject;
                highlight.GetComponent<Image>().overrideSprite = Assets.Texture2D.texUIHighlightHeader.MakeSprite();

                GameObject newHighlight = GameObject.Instantiate(highlight, highlight.transform.parent);
                newHighlight.name = "SelectedHighlight";

                SurvivorVariantButton.imageOnHover = newHighlight.GetComponent<Image>();

                SurvivorVariantAllocator = new(SurvivorVariantContainer.GetComponent<RectTransform>(), SkillPrefab);
                SurvivorVariantFillerAllocator = new(SurvivorVariantContainer.GetComponent<RectTransform>(), SkillFillerPrefab.gameObject);
            }

            public void DisplaySurvivorVariant() {
                active = true;
                SurvivorVariantButton.transform.parent.GetComponent<HGHeaderNavigationController>().ChooseHeaderByButton(SurvivorVariantButton.GetComponent<MPButton>());
            }

            public void HideSurvivorVariant() {
                active = false;
                SurvivorVariantButton.transform.parent.GetComponent<HGHeaderNavigationController>().ChooseHeaderByButton(CSS.primaryColorImages[1].GetComponent<MPButton>());
            }

            public void PopulateSurvivorVariantForSurvivor(SurvivorDef surv) {
                if (SurvivorVariantCatalog.SurvivorVariantReverseMap.ContainsKey(surv)) {
                    surv = SurvivorVariantCatalog.SurvivorVariantReverseMap[surv];
                }

                List<SurvivorVariantDef> SurvivorVariant = SurvivorVariantCatalog.SurvivorVariantMap[surv];

                int amount = SurvivorVariant.Count + 1;

                SurvivorVariantFillerAllocator.AllocateElements(0);
                SurvivorVariantAllocator.AllocateElements(amount);

                for (int i = 0; i < amount; i++) {
                    SurvivorVariantDef currentSurvivorVariant = i == 0 ? null : SurvivorVariant[i - 1];
                    AllocateSurvivorVariant(i, currentSurvivorVariant ? currentSurvivorVariant.TargetSurvivor : surv, currentSurvivorVariant);
                }

                SurvivorVariantFillerAllocator.AllocateElements(Mathf.Max(0, 4 - amount));
            }

            public void AllocateSurvivorVariant(int element, SurvivorDef surv, SurvivorVariantDef SurvivorVariant) {
                GameObject tmp = SurvivorVariant ? SurvivorVariant.VariantSurvivor.bodyPrefab : surv.bodyPrefab;
                Texture2D rizz = tmp.GetComponent<CharacterBody>().portraitIcon as Texture2D;
                Sprite icon = Sprite.Create(rizz, new Rect(0f, 0f, rizz.width, rizz.height), new Vector2(0.5f, 0.5f), 100f);
                string Display = $"Default";
                string Description = "The default survivor.";

                if (SurvivorVariant) {
                    Display = $"{Language.GetString(surv.displayNameToken)} :: {SurvivorVariant.DisplayName}";
                    Description = $"\"{SurvivorVariant.Description}\"";
                }

                Transform skillStrip = SurvivorVariantAllocator.elements[element];

                Image image = skillStrip.Find("Inner/Icon").GetComponent<Image>();
                HGTextMeshProUGUI name = skillStrip.Find("Inner/SkillDescriptionPanel/SkillName").GetComponent<HGTextMeshProUGUI>();
                HGTextMeshProUGUI description = skillStrip.Find("Inner/SkillDescriptionPanel/SkillDescription").GetComponent<HGTextMeshProUGUI>();
                HGButton button = skillStrip.gameObject.GetComponent<HGButton>();
                Image selectedHighlight = skillStrip.Find("Inner/SelectedHighlight").GetComponent<Image>();

                image.sprite = icon;
                name.text = Display;
                name.color = SurvivorVariant ? SurvivorVariant.Color : surv.primaryColor;
                description.text = Description;

                button.showImageOnHover = true;
                button.disablePointerClick = false;

                if (SurvivorVariant && SurvivorVariant.RequiredUnlock) {
                    bool hasUnlocked = LocalUserManager.GetFirstLocalUser().userProfile.HasUnlockable(SurvivorVariant.RequiredUnlock);
                    
                    if (!hasUnlocked) {
                        rizz = Assets.Texture2D.texUnlockIcon;
                        image.sprite = Sprite.Create(rizz, new Rect(0f, 0f, rizz.width, rizz.height), new Vector2(0.5f, 0.5f), 100f);
                        name.text = "";
                        description.text = SurvivorVariant.RequiredUnlock.getHowToUnlockString();
                        // description.transform.localPosition = new(description.transform.localPosition.x, 97.3f, 0);
                        button.showImageOnHover = false;
                        button.disablePointerClick = true;
                    }
                }

                button.hoverToken = "";

                if ((SurvivorVariant ? SurvivorVariant.VariantSurvivor : surv) == CSS.currentSurvivorDef) {
                    selectedHighlight.color = new Color(selectedHighlight.color.r, selectedHighlight.color.g, selectedHighlight.color.b, 1f);
                }
                else {
                    selectedHighlight.color = new Color32(62, 62, 62, 255);
                }

                SurvivorVariantSlotBehaviour behaviour = button.AddComponent<SurvivorVariantSlotBehaviour>();
                behaviour.surv = SurvivorVariant ? SurvivorVariant.VariantSurvivor : surv;
                behaviour.user = CSS.localUser;

                button.onClick.AddListener(behaviour.OnClick);
            }
        }

        public class SurvivorVariantSlotBehaviour : MonoBehaviour {
            public SurvivorDef surv;
            public LocalUser user;

            public void OnClick() {
                // Debug.Log("clicked buttonm");
                user.userProfile.SetSurvivorPreference(surv);
            }
        }
    }
}