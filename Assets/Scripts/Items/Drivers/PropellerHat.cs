using UnityEngine;

public class PropellerHat : MonoBehaviour
{
    [SerializeField] private Transform spinner;
    [SerializeField] private float rotateSpeed = 0f;

    // Update the hat spin speed *in degrees*. Positive = clockwise, negative = counter-clockwise
    public void setSpinSpeed(float speed) {
        rotateSpeed = speed;
    }

    private void Update()
    {
        spinner.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
    }
}
