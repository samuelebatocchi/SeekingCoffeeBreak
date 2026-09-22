using UnityEngine;

public class SkinApplier : MonoBehaviour
{
    [Header("Animator Controllers per skin")]
    [SerializeField] private RuntimeAnimatorController suitController;      // worker-run_3
    [SerializeField] private RuntimeAnimatorController heroSuitController;  // hero-run_3

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        ApplyEquippedSkin();
    }

    private void ApplyEquippedSkin()
    {
        string equipped = SkinData.GetEquipped(); // "suit" oppure "hero_suit"

        animator.runtimeAnimatorController = (equipped == "hero_suit")
            ? heroSuitController
            : suitController;
    }
}