using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Settings")]
        [Tooltip("How much bigger to grow? 1.1 = 10% bigger")]
        [SerializeField] private float _hoverScale = 1.1f;
        
        [Tooltip("How fast the animation plays")]
        [SerializeField] private float _speed = 15f;

        private Vector3 _originalScale;
        private Vector3 _targetScale;

        private void Start()
        {
            // Remember the starting size so we don't lose it
            _originalScale = transform.localScale;
            _targetScale = _originalScale;
        }

        private void Update()
        {
            // Smoothly animate towards the target size every frame
            transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, Time.deltaTime * _speed);
        }

        // Triggered when mouse enters the button area
        public void OnPointerEnter(PointerEventData eventData)
        {
            _targetScale = _originalScale * _hoverScale;
        }

        // Triggered when mouse leaves the button area
        public void OnPointerExit(PointerEventData eventData)
        {
            _targetScale = _originalScale;
        }
    }