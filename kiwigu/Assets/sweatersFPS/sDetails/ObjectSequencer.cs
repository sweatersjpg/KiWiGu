using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectSequencer : MonoBehaviour
{
    public SequencedElement[] sequence;

    // Start is called before the first frame update

    private void Awake()
    {
        for (int i = 0; i < sequence.Length; i++)
        {
            if (sequence[i].action == Action.Enable) sequence[i].item.SetActive(false);
        }

    }

    void Start()
    {
        
        StartCoroutine(DoSequence());
    }

    public IEnumerator DoSequence()
    {
        for(int i = 0; i < sequence.Length; i++)
        {
            SequencedElement s = sequence[i];
            yield return new WaitForSeconds(s.delay);

            switch(s.action)
            {
                case Action.Enable:
                    s.item.SetActive(true);
                    break;
                case Action.Disable:
                    s.item.SetActive(false);
                    break;
            }
        }
    }

    [System.Serializable]
    public class SequencedElement
    {
        public GameObject item;
        public Action action;
        public float delay;
    }

    public enum Action { Enable, Disable, Instantiate }
}
