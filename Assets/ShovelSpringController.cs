using System.Collections;
using MoreMountains.Feedbacks;
using UnityEngine;

public class ShovelSpringController : MonoBehaviour
{
    [SerializeField] private MMSpringPosition springPosition;
    [SerializeField] private Vector3 moveToValue;
    [SerializeField] private float shovelAnimationDelay;

    public void OnShovelUse()
    {
        if (springPosition != null) StartCoroutine(SpringShovel());
    }

    public IEnumerator SpringShovel()
    {
        springPosition.MoveToSubtractive(moveToValue);
        yield return new WaitForSeconds(shovelAnimationDelay);
        springPosition.MoveToAdditive(moveToValue);
    }
}