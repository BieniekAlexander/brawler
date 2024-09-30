using System.Collections;
using System.Net.NetworkInformation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TransformTests {
    // A Test behaves as an ordinary method
    [Test]
    public void TransformTestsSimplePasses() {
        // Use the Assert class to test conditions
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator TransformTestsWithEnumeratorPasses() {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
    }

    [Test]
    public void TransformTestsTransformationEqualsPasses() {
        TriggerTransformation transformation0 = new(
            new Vector2(0, 1),
            Quaternion.identity,
            Vector3.one
        );

        TriggerTransformation transformation1 = new(
            new Vector2(0, 1),
            Quaternion.identity,
            Vector3.one
        );

        Assert.IsTrue(transformation0 == transformation1);
    }

    [Test]
    public void TransformTestsCoordinatesEqualsPasses() {
        TransformCoordinates coords0 = new(
            Vector3.zero,
            Quaternion.identity,
            Vector3.one
        );

        TransformCoordinates coords1 = new(
            Vector3.zero,
            Quaternion.identity,
            Vector3.one
        );

        Assert.IsTrue(coords0 == coords1);
    }

    [Test]
    public void TransformTestsTransformationToCoordinatesPasses0() {
        Transform origin = new GameObject().transform;
        TriggerTransformation transformation = new(
            new Vector2(0, 1),
            Quaternion.identity,
            Vector3.one
        );

        TransformCoordinates coords = new(
            new Vector3(0, 0, 1),
            Quaternion.identity,
            Vector3.one
        );

        Assert.IsTrue(transformation.ToTransformCoordinates(origin, false)==coords);
    }

    [Test]
    public void TransformTestsCoordinatesToTransformationPasses0() {
        Transform origin = new GameObject().transform;
        TriggerTransformation transformation = new(
            new Vector2(0, 1),
            Quaternion.identity,
            Vector3.one
        );

        TransformCoordinates coords = new(
            new Vector3(0, 0, 1),
            Quaternion.identity,
            Vector3.one
        );

        Assert.IsTrue(TriggerTransformation.FromTransformCoordinates(coords, origin, false)==transformation);
    }

    [Test]
    public void TransformTestsTransformationToCoordinatesPasses1() {
        float offsetMultipler = 2;
        float scaleMultiplier = 5;
        float originRotation = 15f;
        float aimRotation = 20f;
        float finalRotation = 70f;

        Transform origin = new GameObject().transform;
        origin.transform.position = Vector3.one;
        origin.transform.rotation = Quaternion.AngleAxis(originRotation, Vector3.up);

        TriggerTransformation transformation = new(
            new Vector2(aimRotation, offsetMultipler),
            Quaternion.AngleAxis(finalRotation, Vector3.up),
            Vector3.one*scaleMultiplier
        );

        TransformCoordinates coords = new(
            origin.position + new Vector3(
                offsetMultipler*Mathf.Sin((originRotation+aimRotation)*Mathf.Deg2Rad),
                0f,
                offsetMultipler*Mathf.Cos((originRotation+aimRotation)*Mathf.Deg2Rad)
            ),
            origin.transform.rotation*Quaternion.AngleAxis(aimRotation, Vector3.up)*Quaternion.AngleAxis(finalRotation, Vector3.up),
            Vector3.one*scaleMultiplier
        );

        Debug.Log("Calculated Coordinates:\n"+transformation.ToTransformCoordinates(origin, false));
        Debug.Log("Expected Coordinates:\n"+coords);

        Assert.IsTrue(transformation.ToTransformCoordinates(origin, false)==coords);
    }

    [Test]
    public void TransformTestsCoordinatesToTransformationPasses1() {
        float offsetMultipler = 2;
        float scaleMultiplier = 5;
        float originRotation = 15f;
        float aimRotation = 20f;
        float finalRotation = 70f;

        Transform origin = new GameObject().transform;
        origin.transform.position = Vector3.one;
        origin.transform.rotation = Quaternion.AngleAxis(originRotation, Vector3.up);

        TriggerTransformation transformation = new(
            new Vector2(aimRotation, offsetMultipler),
            Quaternion.AngleAxis(finalRotation, Vector3.up),
            Vector3.one*scaleMultiplier
        );

        TransformCoordinates coords = new(
            origin.position + new Vector3(
                offsetMultipler*Mathf.Sin((originRotation+aimRotation)*Mathf.Deg2Rad),
                0f,
                offsetMultipler*Mathf.Cos((originRotation+aimRotation)*Mathf.Deg2Rad)
            ),
            origin.transform.rotation*Quaternion.AngleAxis(aimRotation, Vector3.up)*Quaternion.AngleAxis(finalRotation, Vector3.up),
            Vector3.one*scaleMultiplier
        );

        Debug.Log("Calculated Transformation:\n"+TriggerTransformation.FromTransformCoordinates(coords, origin, false));
        Debug.Log("Expected Transformation:\n"+transformation);

        Assert.IsTrue(TriggerTransformation.FromTransformCoordinates(coords, origin, false)==transformation);
    }
}