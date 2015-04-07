using UnityEngine;
using System.Collections;

namespace Core
{
    public class SmoothFollow : MonoBehaviour
    {
        public Transform target;
        public float distance = 10f;
        public float height = 5f;
        public float heightDamping = 2f;
        public float rotationDamping = 3f;
        public Vector3 correctionPos;
        public Vector3 correctionView;

        void FixedUpdate()
        {
            // Early out if we don't have a target
            if (!target)
                return;

            // Calculate the current rotation angles
            float wantedRotationAngle = target.eulerAngles.y;
            float wantedHeight = target.position.y + height;

            float currentRotationAngle = transform.eulerAngles.y;
            float currentHeight = transform.position.y;

            // Damp the rotation around the y-axis
            currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);

            // Damp the height
            currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);

            // Convert the angle into a rotation
            var currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

            // Set the position of the camera on the x-z plane to:
            // distance meters behind the target
            transform.position = target.position;
            transform.position -= currentRotation * Vector3.forward * distance;

            Vector3 pos = transform.position;
            pos.y = currentHeight;

            // Set the height of the camera
            transform.position = pos + correctionPos;

            GameObject targetTarget = new GameObject("targetHelper");
            targetTarget.transform.position = target.position + correctionView;
            // Always look at the target
            transform.LookAt(targetTarget.transform);
            Destroy(targetTarget);
        }
    }
}
