using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using StratSim.Model;
using DataSources;

namespace StratSimTest
{
    [TestClass]
    public class ValidationUnitTest
    {
        [TestMethod]
        public void TestTypicalValidationData()
        {
            //arrange
            float typical = 0.5F;
            bool expected = false;

            TestValidationInclusiveWithValue(typical, expected);
        }
        [TestMethod]
        public void TestErroneousTooLargeValidationData()
        {
            float erroneousTooLarge = 1.5F;
            bool expected = true;
            TestValidationInclusiveWithValue(erroneousTooLarge, expected);
        }
        [TestMethod]
        public void TestErroneousTooSmallValidationData()
        {
            float erroneousTooSmall = -2F;
            bool expected = true;
            TestValidationInclusiveWithValue(erroneousTooSmall, expected);
        }
        [TestMethod]
        public void TestLowerBoundaryValidationData()
        {
            float lowerBoundary = 0F;
            bool expected = false;
            TestValidationInclusiveWithValue(lowerBoundary, expected);
        }
        [TestMethod]
        public void TestUpperBoundaryValidationData()
        {
            float upperBoundary = 1F;
            bool expected = false;
            TestValidationInclusiveWithValue(upperBoundary, expected);
        }

        //Exclusive
        [TestMethod]
        public void TestTypicalValidationDataExclusive()
        {
            //arrange
            float typical = 0.5F;
            bool expected = false;

            TestValidationExclusiveWithValue(typical, expected);
        }
        [TestMethod]
        public void TestErroneousTooLargeValidationDataExclusive()
        {
            float erroneousTooLarge = 1.5F;
            bool expected = true;
            TestValidationExclusiveWithValue(erroneousTooLarge, expected);
        }
        [TestMethod]
        public void TestErroneousTooSmallValidationDataExclusive()
        {
            float erroneousTooSmall = -2F;
            bool expected = true;
            TestValidationExclusiveWithValue(erroneousTooSmall, expected);
        }
        [TestMethod]
        public void TestLowerBoundaryValidationDataExclusive()
        {
            float lowerBoundary = 0F;
            bool expected = true;
            TestValidationExclusiveWithValue(lowerBoundary, expected);
        }
        [TestMethod]
        public void TestUpperBoundaryValidationDataExclusive()
        {
            float upperBoundary = 1F;
            bool expected = true;
            TestValidationExclusiveWithValue(upperBoundary, expected);
        }


        /// <summary>
        /// Tests the validation command for a given value and compares with the expected result
        /// </summary>
        /// <param name="value">The value to validate</param>
        /// <param name="expectedResult">The expected result of the validation</param>
        void TestValidationInclusiveWithValue(float value, bool expectedResult)
        {
            bool actual;

            actual = RunValidationInclusiveRoutineToTest(value);

            Assert.AreEqual(expectedResult, actual);
        }
        void TestValidationExclusiveWithValue(float value, bool expectedResult)
        {
            bool actual;

            actual = RunValidationExclusiveRoutineToTest(value);

            Assert.AreEqual(expectedResult, actual);
        }

        /// <summary>
        /// Tests the validation of a value inclusively between 0 and 1.
        /// </summary>
        /// <param name="value">The floating point value to test</param>
        /// <returns>True if the value is incorrect</returns>
        bool RunValidationInclusiveRoutineToTest(float value)
        {
            float lowerBound = 0F;
            float upperBound = 1F;
            string field = "";
            bool incorrectValue = false;

            Functions.ValidateBetweenInclusive(value, lowerBound, upperBound, field, ref incorrectValue, false);

            return incorrectValue;
        }

        bool RunValidationExclusiveRoutineToTest(float value)
        {
            float lowerBound = 0F;
            float upperBound = 1F;
            string field = "";
            bool incorrectValue = false;

            Functions.ValidateBetweenExclusive(value, lowerBound, upperBound, field, ref incorrectValue, false);

            return incorrectValue;
        }
    }
}
