using System;
using JetBrains.Annotations;
using NUnit.Framework;

namespace Quarks.Runtime.Tests
{
    [TestFixture]
    public class BuilderTest
    {
        [Test]
        public void Can_Populate_Field()
        {
            var instance = new Builder<FakeClass>().With(t => t.Field, 10).Create();

            Assert.That(instance.Field, Is.EqualTo(10));
        }

        [Test]
        public void Can_Populate_Property()
        {
            var instance = new Builder<FakeClass>().With(t => t.Property, 10).Create();

            Assert.That(instance.Property, Is.EqualTo(10));
        }

        [Test]
        public void Can_Populate_ReadOnly_Property()
        {
            var instance = new Builder<FakeClass>().With(t => t.ReadOnlyProperty, 10).Create();

            Assert.That(instance.ReadOnlyProperty, Is.EqualTo(10));
        }

        [Test]
        public void Can_Populate_Nested_Propery()
        {
            var instance = new Builder<FakeClassWithNestedPropery>().With(t => t.NestedProperty.Property, 10).Create();

            Assert.That(instance.NestedProperty.Property, Is.EqualTo(10));
        }

        [Test]
        public void Can_Be_Constructed_With_Instance()
        {
            var instance = new FakeClass { Field = 10 };

            var created = new Builder<FakeClass>(instance).Create();

            Assert.That(created, Is.SameAs(instance));
        }

        [Test]
        public void Can_Be_Constructed_With_Null_Instance()
        {
            var created = new Builder<FakeClass>(null).Create();

            Assert.That(created, Is.Null);
        }

        [Test]
        public void CanNot_Populate_Method()
        {
            var builder = new Builder<FakeClass>();

            Assert.Throws<InvalidOperationException>(
                () => builder.With(t => t.Method(), 10));
        }

        [Test]
        public void Creating_Class_With_No_ParameterLess_Constructor_Throws_An_Exception()
        {
            Assert.Throws<MissingMethodException>(
               () => new Builder<ClassWithNoParameterlessConstructor>().Create());
        }

        [Test]
        public void Can_Construct_Entity_As_Interface()
        {
            Entity entity = new Entity();

            IEntity result =
                new Builder<IEntity>(entity)
                    .With(x => x.Content, 10)
                    .With(x => x.Id, 20)
                    .Create();

            Assert.That(result, Is.InstanceOf<Entity>());
            Assert.That(result, Is.SameAs(entity));
            Assert.That(entity.Data, Is.EqualTo(10));
            Assert.That(entity.Id, Is.EqualTo(20));
        }

        [Test]
        public void Can_Populate_Child_Class_As_Interface()
        {
            ChildEntity entity = new ChildEntity();

            new Builder<IEntity>(entity)
                .With(x => x.Id, 10)
                .Create();

            Assert.That(entity.Id, Is.EqualTo(10));
        }

        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        private class FakeClass
        {
            public int Field;

            public int Property { get; set; }

            public int ReadOnlyProperty { get; private set; }

            public int Method()
            {
                return 0;
            }
        }

        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        private class FakeClassWithNestedPropery
        {
            public FakeClassWithNestedPropery()
            {
                NestedProperty = new FakeClass();
            }

            public FakeClass NestedProperty { get; set; }
        }

        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        private class ClassWithNoParameterlessConstructor
        {
            public ClassWithNoParameterlessConstructor(int argument)
            {
            }
        }

        public interface IEntity
        {
            object Content { get; set; }

            int Id { get; }
        }

        public class Entity : IEntity
        {
            public int Data { get; set; }

            object IEntity.Content
            {
                get { return Data; }
                set { Data = (int) value; }
            }

            public int Id { get; private set; }
        }

        public class ChildEntity : Entity
        {
            
        }
    }
}