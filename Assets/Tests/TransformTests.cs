using System.Collections;
using System.Drawing.Text;
using System.Net.NetworkInformation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.XR;
using static UnityEngine.UI.Image;

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
    public void TransformTestsTransformationToCoordinatesTrivialPasses() {
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
    public void TransformTestsCoordinatesToTransformationTrivialPasses() {
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
    public void TransformTestsTransformationToCoordinatesPasses() {
        float offsetLength = 2;
        float scaleMultiplier = 5;
        float originRotation = 11f;
        float offsetAngle = 17f;
        float finalRotation = 34f;
        bool mirror = false;

        Transform origin = new GameObject().transform;
        origin.transform.position = Vector3.one;
        origin.transform.rotation = Quaternion.AngleAxis(originRotation, Vector3.up);

        TransformCoordinates calculatedCoords = new TriggerTransformation(
            new Vector2(offsetAngle, offsetLength),
            Quaternion.AngleAxis(finalRotation, Vector3.up),
            Vector3.one*scaleMultiplier
        ).ToTransformCoordinates(origin, mirror);

        TransformCoordinates coords = new(
            origin.position + new Vector3(
                offsetLength*Mathf.Sin((originRotation+offsetAngle)*Mathf.Deg2Rad),
                0f,
                offsetLength*Mathf.Cos((originRotation+offsetAngle)*Mathf.Deg2Rad)
            ),
            origin.transform.rotation*Quaternion.AngleAxis(offsetAngle, Vector3.up)*Quaternion.AngleAxis(finalRotation, Vector3.up),
            Vector3.one*scaleMultiplier
        );

        Debug.Log("Calculated Coordinates:\n"+calculatedCoords);
        Debug.Log("Expected Coordinates:\n"+coords);

        Assert.IsTrue(calculatedCoords==coords);
    }

    [Test]
    public void TransformTestsTransformationToCoordinatesWithMirrorPasses() {
        float offsetLength = 2;
        float scaleMultiplier = 5;
        float originRotation = 11f;
        float offsetAngle = 17f;
        float finalRotation = 34f;
        bool mirror = true;

        Transform origin = new GameObject().transform;
        origin.transform.position = Vector3.one;
        origin.transform.rotation = Quaternion.AngleAxis(originRotation, Vector3.up);

        TransformCoordinates calculatedCoords = new TriggerTransformation(
            new Vector2(offsetAngle, offsetLength),
            Quaternion.AngleAxis(finalRotation, Vector3.up),
            Vector3.one*scaleMultiplier
        ).ToTransformCoordinates(origin, mirror);

        TransformCoordinates coords = new(
            origin.position + new Vector3(
                offsetLength*Mathf.Sin((originRotation+-offsetAngle)*Mathf.Deg2Rad),
                0f,
                offsetLength*Mathf.Cos((originRotation+-offsetAngle)*Mathf.Deg2Rad)
            ),
            origin.transform.rotation*Quaternion.AngleAxis(-offsetAngle, Vector3.up)*Quaternion.AngleAxis(-finalRotation, Vector3.up),
            Vector3.one*scaleMultiplier
        );

        Debug.Log("Calculated Coordinates:\n"+calculatedCoords);
        Debug.Log("Expected Coordinates:\n"+coords);

        Assert.IsTrue(calculatedCoords == coords);
    }

    [Test]
    public void TransformTestsCoordinatesToTransformationPasses() {
        float offsetLength = 2;
        float scaleMultiplier = 5;
        float originRotation = 11f;
        float offsetAngle = 17f;
        float finalRotation = 34f;
        bool mirror = false;

        Transform origin = new GameObject().transform;
        origin.transform.position = Vector3.one;
        origin.transform.rotation = Quaternion.AngleAxis(originRotation, Vector3.up);

        TriggerTransformation transformation = new(
            new Vector2(offsetAngle, offsetLength),
            Quaternion.AngleAxis(finalRotation, Vector3.up),
            Vector3.one*scaleMultiplier
        );

        TransformCoordinates coords = new(
            origin.position + new Vector3(
                offsetLength*Mathf.Sin((originRotation+offsetAngle)*Mathf.Deg2Rad),
                0f,
                offsetLength*Mathf.Cos((originRotation+offsetAngle)*Mathf.Deg2Rad)
            ),
            origin.transform.rotation*Quaternion.AngleAxis(offsetAngle, Vector3.up)*Quaternion.AngleAxis(finalRotation, Vector3.up),
            Vector3.one*scaleMultiplier
        );

        TriggerTransformation calculatedTransformation = TriggerTransformation.FromTransformCoordinates(coords, origin, mirror);

        Debug.Log("Calculated Transformation:\n"+calculatedTransformation);
        Debug.Log("Expected Transformation:\n"+transformation);

        Assert.IsTrue(calculatedTransformation==transformation);
    }

    [Test]
    public void TransformTestsCoordinatesToTransformationWithMirrorPasses() {
        float offsetLength = 2;
        float scaleMultiplier = 5;
        float originRotation = 11f;
        float offsetAngle = 27f;
        float finalRotation = 34f;
        bool mirror = true;

        Transform origin = new GameObject().transform;
        origin.transform.position = Vector3.one;
        origin.transform.rotation = Quaternion.AngleAxis(originRotation, Vector3.up);

        TriggerTransformation calculatedTransformation = TriggerTransformation.FromTransformCoordinates(
            new TransformCoordinates(
                origin.position + new Vector3(
                    offsetLength*Mathf.Sin((originRotation+offsetAngle)*Mathf.Deg2Rad),
                    0f,
                    offsetLength*Mathf.Cos((originRotation+offsetAngle)*Mathf.Deg2Rad)
                ),
                origin.transform.rotation*Quaternion.AngleAxis(offsetAngle, Vector3.up)*Quaternion.AngleAxis(finalRotation, Vector3.up),
                Vector3.one*scaleMultiplier
            ),
            origin,
            mirror
        );

        TriggerTransformation transformation = new(
            new Vector2(-offsetAngle, offsetLength),
            Quaternion.AngleAxis(-finalRotation, Vector3.up),
            Vector3.one*scaleMultiplier
        );

        Debug.Log("Calculated Transformation:\n"+calculatedTransformation);
        Debug.Log("Expected Transformation:\n"+transformation);

        Assert.IsTrue(calculatedTransformation==transformation);
    }

    [Test]
    public void TransformTestsTransformToTransformPasses() {
        float offsetLength = 3;
        float scaleMultiplier = 20;
        float originRotation = 11f;
        float offsetAngle = 17f;
        float finalRotation = 34f;
        bool mirror = false;

        Transform origin = new GameObject().transform;
        origin.transform.position = Vector3.one;
        origin.transform.rotation = Quaternion.AngleAxis(originRotation, Vector3.up);

        TriggerTransformation transformation = new(
            new Vector2(offsetAngle, offsetLength),
            Quaternion.AngleAxis(finalRotation, Vector3.up),
            Vector3.one*scaleMultiplier
        );

        TransformCoordinates calculatedCoords = transformation.ToTransformCoordinates(origin, mirror);
        TriggerTransformation calculatedTransformation = TriggerTransformation.FromTransformCoordinates(calculatedCoords, origin, mirror);

        Debug.Log("Initial Transformation:\n"+transformation);
        Debug.Log("Calculated Coordinates:\n"+calculatedCoords);
        Debug.Log("Calculated Transformation:\n"+calculatedTransformation);
        
        Assert.IsTrue(transformation == calculatedTransformation);
    }

    [Test]
    public void TransformTestsTransformationToTransformationWithMirrorPasses() {
        float offsetLength = 3;
        float scaleMultiplier = 20;
        float originRotation = 11f;
        float offsetAngle = 17f;
        float finalRotation = 34f;
        bool mirror = true;

        Transform origin = new GameObject().transform;
        origin.transform.position = Vector3.one;
        origin.transform.rotation = Quaternion.AngleAxis(originRotation, Vector3.up);

        TriggerTransformation transformation = new(
            new Vector2(offsetAngle, offsetLength),
            Quaternion.AngleAxis(finalRotation, Vector3.up),
            Vector3.one*scaleMultiplier
        );

        TransformCoordinates calculatedCoords = transformation.ToTransformCoordinates(origin, mirror);
        TriggerTransformation calculatedTransformation = TriggerTransformation.FromTransformCoordinates(calculatedCoords, origin, mirror);

        Debug.Log("Initial Transformation:\n"+transformation);
        Debug.Log("Calculated Coordinates:\n"+calculatedCoords);
        Debug.Log("Calculated Transformation:\n"+calculatedTransformation);

        Assert.IsTrue(transformation == calculatedTransformation);
    }

    [Test]
    public void TransformTestsCoordinatesToCoordinatesPasses() {
        float offsetLength = 3;
        float scaleMultiplier = 20;
        float originRotation = 11f;
        float offsetAngle = 17f;
        float finalRotation = 34f;
        bool mirror = false;

        Transform origin = new GameObject().transform;
        origin.transform.position = Vector3.one;
        origin.transform.rotation = Quaternion.AngleAxis(originRotation, Vector3.up);

        TransformCoordinates coords = new(
            origin.position + new Vector3(
                offsetLength*Mathf.Sin((originRotation+offsetAngle)*Mathf.Deg2Rad),
                0f,
                offsetLength*Mathf.Cos((originRotation+offsetAngle)*Mathf.Deg2Rad)
        ),
            origin.transform.rotation*Quaternion.AngleAxis(offsetAngle, Vector3.up)*Quaternion.AngleAxis(finalRotation, Vector3.up),
            Vector3.one*scaleMultiplier
        );

        TriggerTransformation calculatedTransformation = TriggerTransformation.FromTransformCoordinates(coords, origin, mirror);
        TransformCoordinates calculatedCoords = calculatedTransformation.ToTransformCoordinates(origin, mirror);

        Debug.Log("Initial Coordinates:\n"+coords);
        Debug.Log("Calculated Transformation:\n"+calculatedTransformation);
        Debug.Log("Calculated Coordinates:\n"+calculatedCoords);

        Assert.IsTrue(coords == calculatedCoords);
    }

    [Test]
    public void TransformTestsCoordinatesToCoordinatesWithMirrorPasses() {
        float offsetLength = 3;
        float scaleMultiplier = 20;
        float originRotation = 11f;
        float offsetAngle = 17f;
        float finalRotation = 34f;
        bool mirror = true;

        Transform origin = new GameObject().transform;
        origin.transform.position = Vector3.one;
        origin.transform.rotation = Quaternion.AngleAxis(originRotation, Vector3.up);

        TransformCoordinates coords = new(
            origin.position + new Vector3(
                offsetLength*Mathf.Sin((originRotation+offsetAngle)*Mathf.Deg2Rad),
                0f,
                offsetLength*Mathf.Cos((originRotation+offsetAngle)*Mathf.Deg2Rad)
        ),
            origin.transform.rotation*Quaternion.AngleAxis(offsetAngle, Vector3.up)*Quaternion.AngleAxis(finalRotation, Vector3.up),
            Vector3.one*scaleMultiplier
        );

        TriggerTransformation calculatedTransformation = TriggerTransformation.FromTransformCoordinates(coords, origin, mirror);
        TransformCoordinates calculatedCoords = calculatedTransformation.ToTransformCoordinates(origin, mirror);

        Debug.Log("Initial Coordinates:\n"+coords);
        Debug.Log("Calculated Transformation:\n"+calculatedTransformation);
        Debug.Log("Calculated Coordinates:\n"+calculatedCoords);

        Assert.IsTrue(coords == calculatedCoords);
    }
}