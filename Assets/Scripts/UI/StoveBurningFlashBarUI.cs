using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoveBurningFlashBarUI : MonoBehaviour
{
    private const string IS_FLASHING = "IsFlashing";

    [SerializeField] private StoveCounter stoveCounter;

    private Animator animator;

    private void Awake() {
        animator = GetComponent<Animator>();
    }

    private void Start() {
        stoveCounter.OnProgressChanged += StoveCounter_OnProgressChanged;
        animator.SetBool(IS_FLASHING, false);
    }

    private void StoveCounter_OnProgressChanged(object sender, IHasProgress.OnProgressEventArgs e) {
        float burnShowProgressAmount = 0.5f;
        bool isFlashing = stoveCounter.IsFried() && e.progressNormalized >= burnShowProgressAmount;

        animator.SetBool(IS_FLASHING, !isFlashing);
    }
}
