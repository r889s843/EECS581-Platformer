using UnityEngine;

namespace Scripts
{
    public class RandomAnimation : MonoBehaviour
    {
        private void Start()
        {
            var anim = GetComponent<Animator>();
            var state = anim.GetCurrentAnimatorStateInfo(layerIndex:0);
            anim.Play(state.fullPathHash, layer:0, normalizedTime:Random.Range(0f, 1f));
        }

    }
}