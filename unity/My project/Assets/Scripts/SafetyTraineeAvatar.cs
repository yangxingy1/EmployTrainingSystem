using UnityEngine;

public static class SafetyTraineeAvatar
{
    public static GameObject Create(Transform parent, string name, Vector3 localPosition, float scale = 1f)
    {
        GameObject avatar = new GameObject(name);
        avatar.transform.SetParent(parent, false);
        avatar.transform.localPosition = localPosition;
        avatar.transform.localRotation = Quaternion.identity;
        avatar.transform.localScale = Vector3.one * Mathf.Max(0.1f, scale);

        CreatePart(avatar.transform, "Soft Ground Shadow", PrimitiveType.Cylinder,
            new Vector3(0f, -0.985f, 0f), Quaternion.identity, new Vector3(0.54f, 0.01f, 0.38f),
            new Color(0.035f, 0.040f, 0.045f));

        GameObject leftBoot = CreatePart(avatar.transform, "Left Safety Boot", PrimitiveType.Cube,
            new Vector3(-0.17f, -0.86f, 0.07f), Quaternion.Euler(0f, 4f, 0f), new Vector3(0.24f, 0.18f, 0.34f),
            new Color(0.045f, 0.050f, 0.056f));
        GameObject rightBoot = CreatePart(avatar.transform, "Right Safety Boot", PrimitiveType.Cube,
            new Vector3(0.17f, -0.86f, 0.07f), Quaternion.Euler(0f, -4f, 0f), new Vector3(0.24f, 0.18f, 0.34f),
            new Color(0.045f, 0.050f, 0.056f));

        GameObject leftLeg = CreatePart(avatar.transform, "Left Work Pant Leg", PrimitiveType.Capsule,
            new Vector3(-0.16f, -0.50f, 0f), Quaternion.Euler(0f, 0f, -2f), new Vector3(0.145f, 0.36f, 0.145f),
            new Color(0.10f, 0.22f, 0.36f));
        GameObject rightLeg = CreatePart(avatar.transform, "Right Work Pant Leg", PrimitiveType.Capsule,
            new Vector3(0.16f, -0.50f, 0f), Quaternion.Euler(0f, 0f, 2f), new Vector3(0.145f, 0.36f, 0.145f),
            new Color(0.10f, 0.22f, 0.36f));

        CreatePart(avatar.transform, "Tool Belt", PrimitiveType.Cube,
            new Vector3(0f, -0.13f, 0f), Quaternion.identity, new Vector3(0.61f, 0.11f, 0.36f),
            new Color(0.055f, 0.060f, 0.065f));
        CreatePart(avatar.transform, "Belt Buckle", PrimitiveType.Cube,
            new Vector3(0f, -0.12f, 0.205f), Quaternion.identity, new Vector3(0.12f, 0.08f, 0.025f),
            new Color(0.78f, 0.62f, 0.22f));
        CreatePart(avatar.transform, "Work Jacket", PrimitiveType.Cube,
            new Vector3(0f, 0.22f, 0f), Quaternion.identity, new Vector3(0.62f, 0.62f, 0.36f),
            new Color(0.07f, 0.24f, 0.42f));

        CreatePart(avatar.transform, "Safety Vest Front", PrimitiveType.Cube,
            new Vector3(0f, 0.23f, 0.193f), Quaternion.identity, new Vector3(0.49f, 0.57f, 0.035f),
            new Color(0.96f, 0.47f, 0.08f));
        CreatePart(avatar.transform, "Safety Vest Back", PrimitiveType.Cube,
            new Vector3(0f, 0.23f, -0.193f), Quaternion.identity, new Vector3(0.49f, 0.57f, 0.035f),
            new Color(0.96f, 0.47f, 0.08f));
        CreatePart(avatar.transform, "Vest Front Reflector", PrimitiveType.Cube,
            new Vector3(0f, 0.33f, 0.216f), Quaternion.identity, new Vector3(0.54f, 0.055f, 0.018f),
            new Color(1f, 0.89f, 0.20f));
        CreatePart(avatar.transform, "Vest Back Reflector", PrimitiveType.Cube,
            new Vector3(0f, 0.33f, -0.216f), Quaternion.identity, new Vector3(0.54f, 0.055f, 0.018f),
            new Color(1f, 0.89f, 0.20f));
        CreatePart(avatar.transform, "Name Badge", PrimitiveType.Cube,
            new Vector3(-0.16f, 0.43f, 0.223f), Quaternion.identity, new Vector3(0.16f, 0.09f, 0.018f),
            new Color(0.55f, 0.88f, 1f));

        GameObject leftArm = CreatePart(avatar.transform, "Left Sleeve", PrimitiveType.Capsule,
            new Vector3(-0.46f, 0.18f, 0f), Quaternion.Euler(0f, 0f, -13f), new Vector3(0.13f, 0.35f, 0.13f),
            new Color(0.06f, 0.22f, 0.39f));
        GameObject rightArm = CreatePart(avatar.transform, "Right Sleeve", PrimitiveType.Capsule,
            new Vector3(0.46f, 0.18f, 0f), Quaternion.Euler(0f, 0f, 13f), new Vector3(0.13f, 0.35f, 0.13f),
            new Color(0.06f, 0.22f, 0.39f));
        GameObject leftGlove = CreatePart(avatar.transform, "Left Glove", PrimitiveType.Sphere,
            new Vector3(-0.54f, -0.20f, 0.02f), Quaternion.identity, new Vector3(0.18f, 0.15f, 0.18f),
            new Color(0.94f, 0.74f, 0.48f));
        GameObject rightGlove = CreatePart(avatar.transform, "Right Glove", PrimitiveType.Sphere,
            new Vector3(0.54f, -0.20f, 0.02f), Quaternion.identity, new Vector3(0.18f, 0.15f, 0.18f),
            new Color(0.94f, 0.74f, 0.48f));

        CreatePart(avatar.transform, "Neck", PrimitiveType.Cylinder,
            new Vector3(0f, 0.56f, 0f), Quaternion.identity, new Vector3(0.12f, 0.08f, 0.12f),
            new Color(0.86f, 0.62f, 0.42f));
        CreatePart(avatar.transform, "Head", PrimitiveType.Sphere,
            new Vector3(0f, 0.75f, 0.02f), Quaternion.identity, new Vector3(0.38f, 0.39f, 0.36f),
            new Color(0.91f, 0.68f, 0.48f));
        CreatePart(avatar.transform, "Safety Glasses", PrimitiveType.Cube,
            new Vector3(0f, 0.79f, 0.214f), Quaternion.identity, new Vector3(0.25f, 0.052f, 0.018f),
            new Color(0.08f, 0.13f, 0.16f));
        CreatePart(avatar.transform, "Helmet Crown", PrimitiveType.Sphere,
            new Vector3(0f, 0.95f, 0.02f), Quaternion.identity, new Vector3(0.45f, 0.18f, 0.43f),
            new Color(1f, 0.77f, 0.12f));
        CreatePart(avatar.transform, "Helmet Front Brim", PrimitiveType.Cube,
            new Vector3(0f, 0.89f, 0.24f), Quaternion.identity, new Vector3(0.50f, 0.045f, 0.13f),
            new Color(0.96f, 0.68f, 0.08f));
        CreatePart(avatar.transform, "Helmet Rear Brim", PrimitiveType.Cube,
            new Vector3(0f, 0.89f, -0.18f), Quaternion.identity, new Vector3(0.42f, 0.04f, 0.10f),
            new Color(0.96f, 0.68f, 0.08f));
        CreatePart(avatar.transform, "Helmet Ridge", PrimitiveType.Cube,
            new Vector3(0f, 1.025f, 0.02f), Quaternion.identity, new Vector3(0.09f, 0.045f, 0.38f),
            new Color(1f, 0.84f, 0.20f));

        SafetyTraineeAvatarWalker walker = avatar.AddComponent<SafetyTraineeAvatarWalker>();
        walker.leftLeg = leftLeg.transform;
        walker.rightLeg = rightLeg.transform;
        walker.leftBoot = leftBoot.transform;
        walker.rightBoot = rightBoot.transform;
        walker.leftArm = leftArm.transform;
        walker.rightArm = rightArm.transform;
        walker.leftGlove = leftGlove.transform;
        walker.rightGlove = rightGlove.transform;

        return avatar;
    }

    static GameObject CreatePart(Transform parent, string name, PrimitiveType primitive, Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Color color)
    {
        GameObject part = GameObject.CreatePrimitive(primitive);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localRotation = localRotation;
        part.transform.localScale = localScale;

        Renderer renderer = part.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = renderer.material;
            material.color = color;
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", 0.32f);
            }
            if (material.HasProperty("_Glossiness"))
            {
                material.SetFloat("_Glossiness", 0.32f);
            }
        }

        Collider collider = part.GetComponent<Collider>();
        if (collider != null)
        {
            Object.Destroy(collider);
        }

        return part;
    }
}

public class SafetyTraineeAvatarWalker : MonoBehaviour
{
    public Transform leftLeg;
    public Transform rightLeg;
    public Transform leftBoot;
    public Transform rightBoot;
    public Transform leftArm;
    public Transform rightArm;
    public Transform leftGlove;
    public Transform rightGlove;

    public float swingDegrees = 50f;
    public float armSwingDegrees = 42f;
    public float bootLift = 0.14f;
    public float strideDistance = 0.16f;
    public float armStrideDistance = 0.23f;
    public float blendInSpeed = 8f;
    public float blendOutSpeed = 10f;

    CharacterController _controller;
    bool _poseCaptured;
    float _walkBlend;
    float _phase;
    Vector3 _lastParentPosition;
    bool _hasLastParentPosition;
    Quaternion _leftLegRestRotation;
    Quaternion _rightLegRestRotation;
    Quaternion _leftBootRestRotation;
    Quaternion _rightBootRestRotation;
    Quaternion _leftArmRestRotation;
    Quaternion _rightArmRestRotation;
    Quaternion _leftGloveRestRotation;
    Quaternion _rightGloveRestRotation;
    Vector3 _leftBootRestPosition;
    Vector3 _rightBootRestPosition;
    Vector3 _leftGloveRestPosition;
    Vector3 _rightGloveRestPosition;

    void LateUpdate()
    {
        if (!_poseCaptured)
        {
            CapturePose();
        }

        float speed = ResolvePlanarSpeed();
        float targetBlend = speed > 0.08f ? Mathf.Clamp01(speed / 4.0f) : 0f;
        float blendSpeed = targetBlend > _walkBlend ? blendInSpeed : blendOutSpeed;
        _walkBlend = Mathf.MoveTowards(_walkBlend, targetBlend, Time.deltaTime * blendSpeed);

        if (_walkBlend > 0.001f)
        {
            _phase += Time.deltaTime * Mathf.Lerp(6.0f, 11.2f, Mathf.Clamp01(speed / 6.2f));
        }

        ApplyWalkPose(Mathf.Sin(_phase), Mathf.Cos(_phase));
    }

    void CapturePose()
    {
        _controller = GetComponentInParent<CharacterController>();
        _leftLegRestRotation = RotationOf(leftLeg);
        _rightLegRestRotation = RotationOf(rightLeg);
        _leftBootRestRotation = RotationOf(leftBoot);
        _rightBootRestRotation = RotationOf(rightBoot);
        _leftArmRestRotation = RotationOf(leftArm);
        _rightArmRestRotation = RotationOf(rightArm);
        _leftGloveRestRotation = RotationOf(leftGlove);
        _rightGloveRestRotation = RotationOf(rightGlove);
        _leftBootRestPosition = PositionOf(leftBoot);
        _rightBootRestPosition = PositionOf(rightBoot);
        _leftGloveRestPosition = PositionOf(leftGlove);
        _rightGloveRestPosition = PositionOf(rightGlove);
        _lastParentPosition = transform.parent != null ? transform.parent.position : transform.position;
        _hasLastParentPosition = true;
        _poseCaptured = true;
    }

    float ResolvePlanarSpeed()
    {
        Vector3 velocity = _controller != null ? _controller.velocity : Vector3.zero;
        velocity.y = 0f;
        if (velocity.sqrMagnitude > 0.0001f)
        {
            return velocity.magnitude;
        }

        Vector3 parentPosition = transform.parent != null ? transform.parent.position : transform.position;
        if (!_hasLastParentPosition)
        {
            _lastParentPosition = parentPosition;
            _hasLastParentPosition = true;
            return 0f;
        }

        Vector3 delta = parentPosition - _lastParentPosition;
        _lastParentPosition = parentPosition;
        delta.y = 0f;
        return delta.magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
    }

    void ApplyWalkPose(float sine, float cosine)
    {
        float legSwing = sine * swingDegrees * _walkBlend;
        float armSwing = sine * armSwingDegrees * _walkBlend;
        float leftLift = Mathf.Max(0f, cosine) * bootLift * _walkBlend;
        float rightLift = Mathf.Max(0f, -cosine) * bootLift * _walkBlend;
        float footStride = sine * strideDistance * _walkBlend;
        float handStride = sine * armStrideDistance * _walkBlend;

        SetRotation(leftLeg, _leftLegRestRotation * Quaternion.Euler(legSwing, 0f, 0f));
        SetRotation(rightLeg, _rightLegRestRotation * Quaternion.Euler(-legSwing, 0f, 0f));
        SetRotation(leftBoot, _leftBootRestRotation * Quaternion.Euler(legSwing * 0.62f, 0f, 0f));
        SetRotation(rightBoot, _rightBootRestRotation * Quaternion.Euler(-legSwing * 0.62f, 0f, 0f));
        SetRotation(leftArm, _leftArmRestRotation * Quaternion.Euler(-armSwing, 0f, 0f));
        SetRotation(rightArm, _rightArmRestRotation * Quaternion.Euler(armSwing, 0f, 0f));
        SetRotation(leftGlove, _leftGloveRestRotation * Quaternion.Euler(-armSwing * 0.55f, 0f, 0f));
        SetRotation(rightGlove, _rightGloveRestRotation * Quaternion.Euler(armSwing * 0.55f, 0f, 0f));

        SetPosition(leftBoot, _leftBootRestPosition + Vector3.forward * footStride + Vector3.up * leftLift);
        SetPosition(rightBoot, _rightBootRestPosition - Vector3.forward * footStride + Vector3.up * rightLift);
        SetPosition(leftGlove, _leftGloveRestPosition - Vector3.forward * handStride + Vector3.up * rightLift * 0.55f);
        SetPosition(rightGlove, _rightGloveRestPosition + Vector3.forward * handStride + Vector3.up * leftLift * 0.55f);
    }

    static Quaternion RotationOf(Transform target)
    {
        return target != null ? target.localRotation : Quaternion.identity;
    }

    static Vector3 PositionOf(Transform target)
    {
        return target != null ? target.localPosition : Vector3.zero;
    }

    static void SetRotation(Transform target, Quaternion rotation)
    {
        if (target != null)
        {
            target.localRotation = rotation;
        }
    }

    static void SetPosition(Transform target, Vector3 position)
    {
        if (target != null)
        {
            target.localPosition = position;
        }
    }
}
