using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Dropdown))]
[DisallowMultipleComponent]
public class DropDownController : MonoBehaviour, IPointerClickHandler
{
    [Tooltip("Indexes that should be ignored. Indexes are 0 based.")]
    public List<int> indexesToDisable = new List<int>();

    private Dropdown _dropdown;
    [SerializeField]
    private ScrollRect _scrollRect;

    private void Awake()
    {
        _dropdown = GetComponent<Dropdown>();
    }
    //private void Start()
    //{
    //    _scrollRect = _dropdown.transform.Find("Dropdown List").GetComponent<ScrollRect>();
    //}

    public void OnPointerClick(PointerEventData eventData)
    {

        //Set Scrollbar Value - For Displaying last message of content
        Scrollbar scrollBar = transform.Find("Dropdown List").GetChild(1).GetComponent<Scrollbar>();


        //print(scrollBar.name);

        int dropdownListCount = _dropdown.options.Count;
        int dropDownCurrentValue = _dropdown.value;

        Canvas.ForceUpdateCanvases();


        float calculatedValue = (float)Map(dropDownCurrentValue, 0, dropdownListCount, 1, 0);

        //print("Value = " + (float)Map(dropDownCurrentValue, 0, dropdownListCount, 1, 0));      

        scrollBar.value = calculatedValue > 0.9f ? 1 : calculatedValue < 0.1f ? 0 : calculatedValue;
        Canvas.ForceUpdateCanvases();

    }

    public static double Map(double x, double in_min, double in_max, double out_min, double out_max, bool clamp = false)
    {
        if (clamp) x = System.Math.Max(in_min, System.Math.Min(x, in_max));
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }

    private void ScrollToCurrentElement()
    {
        var siblingIndex = _dropdown.value;

        float pos = 1f - (float)siblingIndex / _scrollRect.content.transform.childCount;

        if (pos < 0.4)
        {
            float correction = 1f / _scrollRect.content.transform.childCount;
            pos -= correction;
        }

        _scrollRect.verticalNormalizedPosition = pos;
    }

    // Anytime change a value by index
    public void EnableOption(int index, bool enable)
    {
        if (index < 1 || index > _dropdown.options.Count)
        {
            Debug.LogWarning("Index out of range -> ignored!", this);
            return;
        }

        if (enable)
        {
            // remove index from disabled list
            if (indexesToDisable.Contains(index)) indexesToDisable.Remove(index);
        }
        else
        {
            // add index to disabled list
            if (!indexesToDisable.Contains(index)) indexesToDisable.Add(index);
        }

        var dropDownList = GetComponentInChildren<Canvas>();

        // If this returns null than the Dropdown was closed
        if (!dropDownList) return;

        // If the dropdown was opened find the options toggles
        var toogles = dropDownList.GetComponentsInChildren<Toggle>(true);
        toogles[index].interactable = enable;
    }

    // Anytime change a value by string label
    public void EnableOption(string label, bool enable)
    {
        var index = _dropdown.options.FindIndex(o => string.Equals(o.text, label));

        // We need a 1-based index
        EnableOption(index + 1, enable);
    }
}