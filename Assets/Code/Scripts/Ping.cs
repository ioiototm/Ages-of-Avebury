using System.Collections;
using System.Linq;
using UnityEngine;

public class Ping : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(Pulse(2.0f));
    }

    // Update is called once per frame
    void Update()
    {

        //if i press t, start the pulse coroutine
        if (Input.GetKeyDown(KeyCode.T))
        {
            StartCoroutine(Pulse());
        }

    }

    IEnumerator Pulse(float duration = 0.9f)
    {
        var t = 0f;
        var startScale = 0.01f;
        var endScale = 0.5f;

        var mats = GetComponentsInChildren<Renderer>()
                   .Select(r => r.material).ToArray();

        while (t < duration)
        {
            t += Time.deltaTime;
            float k = t / duration;
            float scale = Mathf.Lerp(startScale, endScale, k);
            transform.localScale = Vector3.one * scale;

            float alpha = Mathf.Lerp(1f, 0f, k);
            foreach (var m in mats)
            {
                var c = m.color;
                c.a = alpha;
                m.color = c;
            }
            yield return null;
        }
        gameObject.Destroy();
        //gameObject.SetActive(false); // or Destroy
    }

}
