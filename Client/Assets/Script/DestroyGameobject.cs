using UnityEngine;
using System.Collections;

public class DestroyGameobject : MonoBehaviour
{
    private TweenPosition mtween;
    // Use this for initialization
    void Awake()
    {
        mtween = gameObject.GetComponent<TweenPosition>();
        mtween.onFinished.Add(new EventDelegate(DestroyLabel));
    }

    //// Update is called once per frame
    //void Update()
    //{
        
    //}
    void DestroyLabel()
    {
        Destroy(gameObject);
    }

    void OnDisable()
    {
        Destroy(gameObject);
    }
}