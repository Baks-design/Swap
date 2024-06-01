using UnityEngine;

public class NPCVFX : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Transform[] eyes;

    void Update()
    {
        for (var i = 0; i < eyes.Length; i++)
            eyes[i].LookAt(target.transform);
    }
}
