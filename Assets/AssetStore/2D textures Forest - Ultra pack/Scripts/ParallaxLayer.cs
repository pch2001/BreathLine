using UnityEngine;

namespace Forest2D
{
    [ExecuteInEditMode]
    public class ParallaxLayer : MonoBehaviour
    {
        [SerializeField] float multiplier = 0.0f;
        [SerializeField] Mode direction = 0;

        private Transform cameraTransform;

        private Vector3 startCameraPos;
        private Vector3 startPos;

        private enum Mode
        {
            Horizontal,
            Vertical,
            All
        }

        void Start()
        {
            cameraTransform = Camera.main.transform;
            startCameraPos = cameraTransform.position;
            startPos = transform.position;
        }


        private void LateUpdate()
        {
            var position = startPos;

            switch (direction)
            {
                case Mode.Horizontal:
                    position.x += multiplier * (cameraTransform.position.x - startCameraPos.x);
                    break;
                case Mode.Vertical:
                    position.y += multiplier * (cameraTransform.position.y - startCameraPos.y);
                    break;
                case Mode.All:
                    position += multiplier * (cameraTransform.position - startCameraPos);
                    break;
            }

            transform.position = position;
        }

    }
}