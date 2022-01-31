using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace TetraPak.XP.DependencyInjection.Tests
{
    public class XpServicesTests
    {
        [Fact]
        public void Ensure_can_only_register_concrete_types()
        {
            XpServices.Reset();
            Assert.Throws<ArgumentException>(XpServices.Register<ITestInterface1>);
            Assert.Throws<ArgumentException>(XpServices.Register<AbstractTestClass>);
            XpServices.Register<TestClass>();
        }

        [Fact]
        public void Ensure_literal_service_can_only_be_resolved_literally()
        {
            XpServices.Reset();
            XpServices.RegisterLiteral<TestClass>();
            Assert.False(XpServices.TryGet<ITestInterface1>());
            Assert.False(XpServices.TryGet<AbstractTestClass>());
            var outcome = XpServices.TryGet<TestClass>();
            Assert.True(outcome);
            Assert.IsType<TestClass>(outcome.Value);
        }
        
        [Fact]
        public void Ensure_non_literal_service_can_be_resolved_from_abstract_Type()
        {
            XpServices.Reset();
            XpServices.Register<TestClass>();

            var outcome1 = XpServices.TryGet<ITestInterface1>();
            Assert.IsType<TestClass>(outcome1.Value);
            
            var outcome2 = XpServices.TryGet<AbstractTestClass>();
            Assert.IsType<TestClass>(outcome2.Value);

            var outcome3 = XpServices.TryGet<TestClass>();
            Assert.IsType<TestClass>(outcome3.Value);
        }

        [Fact]
        public void Ensure_first_registered_service_gets_resolved_for_common_abstract_type()
        {
            // register derived class before base class and ensure requesting interface resolves derived class ...
            XpServices.Reset();
            XpServices.Register<DerivedTestClass>();
            XpServices.Register<TestClass>();
            var outcome1 = XpServices.TryGet<ITestInterface1>();
            Assert.IsType<DerivedTestClass>(outcome1.Value);

            // now register base class before derived class and ensure it gets resolved for common interface ...
            XpServices.Reset();
            XpServices.Register<TestClass>();
            XpServices.Register<DerivedTestClass>();
            outcome1 = XpServices.TryGet<ITestInterface1>();
            Assert.IsType<TestClass>(outcome1.Value);
            
            // finally, ensure derived class gets resolved for interface only it implements ... 
            var outcome2 = XpServices.TryGet<ITestInterface2>();
            Assert.IsType<DerivedTestClass>(outcome2.Value);
        }
        
        [Fact]
        public void Ensure_literal_service_gets_resolved_to_literal_request()
        {
            // register derived class before base class and ensure requesting interface resolves derived class ...
            XpServices.Reset();
            XpServices.Register<DerivedTestClass>();
            XpServices.Register<TestClass>();
            var outcome1 = XpServices.TryGet<ITestInterface1>();
            Assert.IsType<DerivedTestClass>(outcome1.Value);
            var outcome2 = XpServices.TryGet<TestClass>();
            Assert.IsType<TestClass>(outcome2.Value);
        }

        [Fact]
        public static void Ensure_combined_XpService_and_IServiceProvider_resolves_correct()
        {
            XpServices.Reset();
            var collection = XpServices.GetServiceCollection();
            XpServices.Register<TestClass>();
            collection.AddSingleton<ITestInterface1, DerivedTestClass>();
            var provider = collection.BuildXpServiceProvider();
            Assert.IsType<DerivedTestClass>(provider.GetService<ITestInterface1>());
            Assert.Null(provider.GetService<ITestInterface2>());
            Assert.IsType<TestClass>(provider.GetService<TestClass>());
            
            // now test resolving from XpService ...
            Assert.IsType<DerivedTestClass>(XpServices.Get<ITestInterface1>());
            Assert.Null(XpServices.Get<ITestInterface2>());
            Assert.IsType<TestClass>(XpServices.Get<TestClass>());
        }
    }
    
    public interface ITestInterface1
    {}
    
    public interface ITestInterface2
    {}
    
    public abstract class AbstractTestClass : ITestInterface1
    {}
    
    public class TestClass : AbstractTestClass
    {}
    
    public class DerivedTestClass : TestClass, ITestInterface2
    {}
}