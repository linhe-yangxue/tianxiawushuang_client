using UnityEngine;


public class LuaBehaviourHelper : MonoBehaviour
{
    public LuaBehaviour behaviour;

    private void Start()
    {
        behaviour.Start();
    }

    private void Update()
    {
        behaviour.Update(Time.deltaTime);
    }

    private void OnDisable()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        behaviour.OnDestroy();
    }
}