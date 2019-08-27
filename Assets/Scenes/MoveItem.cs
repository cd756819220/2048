using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveItem : MonoBehaviour
{
    private Image _image;
    private Text _text;
    private Animation _animation;
    private bool _isChanged;
    private int _index = -1;
    private Color _colorBg;
    private Color _colorText;

    public int index
    {
        get
        {
            return _index;
        }
        set
        {
            this._index = value;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        this._animation = this.GetComponent<Animation>();
        this._isChanged = false;
        this._image = this.GetComponent<Image>();
        this._text = this.transform.Find("Text").GetComponent<Text>();
        this._updateShow();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        if (this._animation)
        {
            //this._animation.clip = this._animation.GetClip("ItemBorn");
            this._animation.Play("ItemBorn");
        }
    }

    public void setMoveItemValue(int index, Color colorBg, Color colorText)
    {
        this._isChanged = this._index != -1 && this._index != index;
        this._index = index;
        this._colorBg = colorBg;
        this._colorText = colorText;
        this._updateShow();
    }

    private void _updateShow()
    {
        if (this._index == -1) return;
        if (this._image)
        {
            this._image.color = this._colorBg;
            if (this._isChanged)
            {
                this._animation.Play("ItemChanged");
                this._isChanged = false;
            }
        }
        if (this._text)
        {
            this._text.text = System.Convert.ToString(Mathf.Pow(2, this._index));
            this._text.color = this._colorText;
        }
    }
}
