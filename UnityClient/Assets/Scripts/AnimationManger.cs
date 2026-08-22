using System.Collections;
using UnityEngine;

public class AnimationManger : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private AnimationClip[] idleAnimations;

    private Coroutine idleCoroutine;
    private bool gesturePlaying;

    void Start()
    {
        idleCoroutine = StartCoroutine(RandomIdle());
    }

    IEnumerator RandomIdle()
    {
        while (true)
        {
            if (!gesturePlaying)
            {
                AnimationClip clip =
                    idleAnimations[Random.Range(0, idleAnimations.Length)];

                animator.CrossFade(clip.name, 0.2f);

                float time = 0;

                // Clip duration tak wait, lekin gesture aaye to turant exit
                while (time < clip.length && !gesturePlaying)
                {
                    time += Time.deltaTime;
                    yield return null;
                }
            }
            else
            {
                yield return null;
            }
        }
    }

    public void PlayGesture(string gesture)
    {
        Debug.Log("Playing Gesture: " + gesture);

        if (gesturePlaying)
            return;

        if (gesture == "greeting")
            StartCoroutine(PlayGestureAnimation("Quick Formal Bow"));
        else if (gesture == "goodbye")
            StartCoroutine(PlayGestureAnimation("Waving"));
    }

    IEnumerator PlayGestureAnimation(string animationName)
    {
        // Idle ko immediately block karo
        gesturePlaying = true;

        // Bow/Bye play karo
        animator.CrossFade(animationName, 0.15f);

        // CrossFade complete hone do
        yield return new WaitForSeconds(0.2f);

        // Gesture state ki actual length lo
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        float duration = state.length;

        // Gesture complete hone do
        yield return new WaitForSeconds(duration);

        gesturePlaying = false;
    }
}