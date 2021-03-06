﻿using System;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinterTests.TestClasses;
using ObjectPrinting;
using ObjectPrinting.Config.Member;
using ObjectPrinting.Config.Type;

namespace ObjectPrinterTests
{
    [TestFixture]
    public class ObjectPrinterShould
    {
        private const string Guid = "0f8fad5b-d9cb-469f-a165-70867728950e";

        private readonly CultureInfo englishCulture =
            CultureInfo.GetCultures(CultureTypes.AllCultures).First(c => c.EnglishName == "English");

        private Person person;

        [SetUp]
        public void SetUp()
        {
            person = new Person(Guid);
        }

        [Test]
        public void PrintNull()
        {
            var printer = ObjectPrinter.For<Person>();

            printer.PrintToString(null).Should().Be("null");
        }

        [Test]
        public void ExcludeTypes()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding<int>()
                .Excluding<Guid>();

            printer.PrintToString(person).Should().NotContainAny("Id", "Age1");
        }

        [Test]
        public void AllowToOverrideTypesPrinting()
        {

            var printer = ObjectPrinter.For<Person>()
                .Printing<Guid>().Using(guid => $"GUID: {guid}")
                .Printing<int>().Using(i => $"int: {i}");

            printer.PrintToString(person).Should()
                .ContainAll($"Id = GUID: {Guid}", "Age1 = int: 21")
                .And.NotContainAny("Age1 = 21", $"Id = {Guid}");
        }

        [Test]
        public void AllowToOverrideTypesCulture()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<TimeSpan>().Using(englishCulture)
                .Printing<DateTime>().Using(englishCulture);

            printer.PrintToString(person).Should()
                .ContainAll("WorkExperience = 3.00:00:00", "Birthday = 1/28/2018")
                .And.NotContainAny("Birthday = 01/28/2018 12:00:00");
        }

        [Test]
        public void AllowToOverridePropertiesPrinting()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Birthday).Using(d => d.Year.ToString());

            printer.PrintToString(person).Should()
                .Contain("Birthday = 2018")
                .And.NotContainAny(person.Birthday.ToString(CultureInfo.InvariantCulture));
        }

        [Test]
        public void AllowToOverrideFieldPrinting()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.SecondName).Using(s => "Egorov");

            printer.PrintToString(person).Should()
                .Contain("SecondName = Egorov")
                .And.NotContain("SecondName = Ivanov");
        }

        [Test]
        public void AllowToTrimStringProperties()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Name).TrimmedToLength(3);

            printer.PrintToString(person).Should()
                .Contain("Name = And")
                .And.NotContain("Andrey");
        }

        [Test]
        public void AllowToTrimStringFields()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.SecondName).TrimmedToLength(3);

            printer.PrintToString(person).Should()
                .Contain("SecondName = Iva")
                .And.NotContain("Ivanov");
        }

        [Test]
        public void TrimStringPropertiesWithBigMaxValues()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Name).TrimmedToLength(1000);

            printer.PrintToString(person).Should()
                .Contain("Name = Andrey");
        }

        [Test]
        public void TrimStringFieldsWithBigMaxValues()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.SecondName).TrimmedToLength(1000);

            printer.PrintToString(person).Should()
                .Contain("SecondName = Ivanov");
        }

        [Test]
        public void ThrowExceptionOnSettingNegativeTrimmingLengthForProperties()
        {
            Action action = () => ObjectPrinter.For<Person>()
                .Printing(p => p.Name).TrimmedToLength(-2);

            action.Should().Throw<ArgumentException>()
                .WithMessage("Trimming length can't be negative");
        }

        [Test]
        public void ThrowExceptionOnSettingNegativeTrimmingLengthForFields()
        {
            Action action = () => ObjectPrinter.For<Person>()
                .Printing(p => p.SecondName).TrimmedToLength(-2);

            action.Should().Throw<ArgumentException>()
                .WithMessage("Trimming length can't be negative");
        }

        [Test]
        public void AllowToExcludeProperties()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding(p => p.Id)
                .Excluding(p => p.Age1);

            printer.PrintToString(person).Should()
                .NotContainAny("Id", "Age1");
        }

        [Test]
        public void AllowToExcludeFields()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding(p => p.SecondName);

            printer.PrintToString(person).Should()
                .NotContain("SecondName");
        }

        [Test]
        public void NotPrintNonPublicFieldsEitherNonPublicProperties()
        {
            var testInstance = new TestClass();

            ObjectPrinter.For<TestClass>().PrintToString(testInstance).Should()
                .ContainAll("PublicField", "PublicProperty")
                .And.NotContainAny(
                    "ProtectedInternalField", "ProtectedInternalProperty", "InternalProperty", "InternalField",
                    "PrivateProperty", "ProtectedField", "PrivateProperty", "privateField");
        }

        [Test]
        public void PrintNestedClasses()
        {
            var b = new B();
            var a = new A
            {
                B1 = b,
                B2 = b
            };

            ObjectPrinter.For<A>().PrintToString(a).Should()
                .ContainAll("B1 = B", "B2 = B");
        }

        [Test]
        public void NotThrowStackOverflowExceptionOnRecursion()
        {
            person.Child = person;
            Action action = () => ObjectPrinter.For<Person>().PrintToString(person);

            action.Should().NotThrow<StackOverflowException>();
        }

        [Test]
        public void SetMaxNestingLevel()
        {
            var child = new Person(Guid);
            child.Name = "Egor";
            person.Child = child;

            var printer = ObjectPrinter.For<Person>()
                .SetMaxNestingLevel(1);

            printer.PrintToString(person).Should()
                .Contain("Name = Andrey")
                .And.NotContain("Name = Egor");
        }
    }
}