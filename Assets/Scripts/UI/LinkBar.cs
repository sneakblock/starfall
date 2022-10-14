using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LinkBar : MonoBehaviour
{
    // List containing a slider for each Link type.
    // [0]: Safe Link
    // [1]: Standard Link
    // [2]: Excess Link
    public List<Slider> _LinkSliders = new List<Slider>();
    // Max values for each Link type. Total Link bar defaults to a max value of 200.
    public float _maxSafeLink = 10, _maxStandardLink = 100, _maxExcessLink = 200;

    // The current Link slider in effect.
    private float _currentLink;
    private Slider _currLinkSlider;
    private int _currSliderNum;

    // Set up each slider's values and set the current Link bar to be composed of Safe Link and Standard Link. 
    void Start()
    {
        SetLinkBarThresholds(_LinkSliders[0], 0, _maxSafeLink);
        SetLinkBarThresholds(_LinkSliders[1], _maxSafeLink, _maxStandardLink);
        SetLinkBarThresholds(_LinkSliders[2], _maxStandardLink, _maxExcessLink);

        SetLinkBarVals(_LinkSliders[0], _maxSafeLink);
        SetLinkBarVals(_LinkSliders[1], _maxStandardLink);
        SetLinkBarVals(_LinkSliders[2], 0);

        _currSliderNum = 1;
        _currLinkSlider = _LinkSliders[_currSliderNum];
        _currentLink = _maxStandardLink;
    }

    // TEMP: manipulate the Link bar's values.
    void Update()
    {
        /*
        if (Input.GetKeyDown(KeyCode.F))
        {
            RemoveLink(30f);
        }
        else if (Input.GetKeyDown(KeyCode.G))
        {
            AddLink(30f);
        }
        */
    }

    // Remove a certain amount of Link from the Link bar.
    public void RemoveLink(float linkAmt)
    {  
        //for some reason this isn't registered as being in Start() when using UnityEvents
        _currLinkSlider = _LinkSliders[_currSliderNum];
        // Update the player's total amount of Link
        if (_currentLink - linkAmt < 0)
        {
            _currentLink = 0;
            // KILL PLAYER
        }
        else
        {
            _currentLink -= linkAmt;
        }

        // if the deducted amount of Link would cross over into the preceding Link region, switch the active Link bar to the preceding one
        if (_currLinkSlider.value - linkAmt < _currLinkSlider.minValue)
        {
            float leftoverLink = _currLinkSlider.minValue - (_currLinkSlider.value - linkAmt);
            UpdateLinkBar(_currLinkSlider.minValue);
            //_currLinkSlider.value = _currLinkSlider.minValue;
            if (_currSliderNum > 0)
            {
                _currSliderNum--;
                _currLinkSlider = _LinkSliders[_currSliderNum];
                RemoveLink(leftoverLink);
            }
            // else, just deduct the Link and display it accordingly on the current active Link region 
        }
        else
        {
            UpdateLinkBar(_currLinkSlider.value - linkAmt);
            //_currLinkSlider.value -= linkAmt;
        }
    }

    // Add a certain amount of Link to the Link Bar.
    public void AddLink(float linkAmt)
    {
        //for some reason this isn't registered as being in Start() when using UnityEvents
        _currLinkSlider = _LinkSliders[_currSliderNum];
        // Update the player's total amount of Link
        if (_currentLink + linkAmt > _maxExcessLink)
        {
            _currentLink = _maxExcessLink;
        }
        else
        {
            _currentLink += linkAmt;
        }

        // if the added amount of Link would cross over into the next Link region, switch the active Link bar to the next one
        if (_currLinkSlider.value + linkAmt > _currLinkSlider.maxValue)
        {
            float leftoverLink = (_currLinkSlider.value + linkAmt) - _currLinkSlider.maxValue;
            //_currLinkSlider.value = _currLinkSlider.maxValue;
            UpdateLinkBar(_currLinkSlider.maxValue);
            if (_currSliderNum < 2)
            {
                _currSliderNum++;
                _currLinkSlider = _LinkSliders[_currSliderNum];
                AddLink(leftoverLink);
            }
        }
        // else, just add the Link and display it accordingly on the current active Link region 
        else
        {
            UpdateLinkBar(_currLinkSlider.value + linkAmt);
            //_currLinkSlider.value += linkAmt;
        }
    }

    // HELPER FUNCTIONS //
    private void UpdateLinkBar(float newLinkAmt)
    {
        _currLinkSlider.value = newLinkAmt;
    }

    private void SetLinkBarThresholds(Slider linkSlider, float minLink, float maxLink)
    {
        linkSlider.minValue = minLink;
        linkSlider.maxValue = maxLink;
    }

    private void SetLinkBarVals(Slider linkSlider, float linkAmt)
    {
        linkSlider.value = linkAmt;
    }
}
