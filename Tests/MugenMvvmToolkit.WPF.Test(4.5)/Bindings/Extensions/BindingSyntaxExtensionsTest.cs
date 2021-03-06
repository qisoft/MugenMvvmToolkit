﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Behaviors;
using MugenMvvmToolkit.Binding.Builders;
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Extensions.Syntax;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Interfaces.Sources;
using MugenMvvmToolkit.Binding.Interfaces.Syntax;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Test.Bindings.Parse;
using MugenMvvmToolkit.Test.TestInfrastructure;
using MugenMvvmToolkit.Test.TestModels;
using Should;

namespace MugenMvvmToolkit.Test.Bindings.Extensions
{
    [TestClass]
    public class BindingSyntaxExtensionsTest : BindingTestBase
    {
        #region Methods

        [TestMethod]
        public void BuilderShouldUseTargetBindingContextForSource1()
        {
            const string targetPath = "Text";
            const string sourcePath = "IntProperty";
            var targetObj = new object();
            var builder = new BindingBuilder();
            builder.Bind(targetObj, targetPath).To(sourcePath);

            IList<Func<IDataContext, IBindingSource>> sources = builder.GetData(BindingBuilderConstants.Sources);
            IBindingSource source = sources.Single().Invoke(builder);
            BindingParserTest.BindingSourceShouldBeValidDataContext(targetObj, source, sourcePath);
            var sourceObj = new BindingSourceModel();
            BindingServiceProvider.ContextManager.GetBindingContext(targetObj).Value = sourceObj;
            BindingParserTest.BindingSourceShouldBeValidDataContext(targetObj, source, sourcePath);
        }

        [TestMethod]
        public void BuilderShouldUseTargetBindingContextForSource2()
        {
            const string targetPath = "Text";
            const string sourcePath = "IntProperty";
            var targetObj = new object();
            var builder = new BindingBuilder();
            builder.Bind(targetObj, targetPath).To<BindingSourceModel>(() => model => model.IntProperty);

            IList<Func<IDataContext, IBindingSource>> sources = builder.GetData(BindingBuilderConstants.Sources);
            IBindingSource source = sources.Single().Invoke(builder);
            BindingParserTest.BindingSourceShouldBeValidDataContext(targetObj, source, sourcePath);
            var sourceObj = new BindingSourceModel();
            BindingServiceProvider.ContextManager.GetBindingContext(targetObj).Value = sourceObj;
            BindingParserTest.BindingSourceShouldBeValidDataContext(targetObj, source, sourcePath);
        }

        [TestMethod]
        public void BuilderShouldUseTargetBindingContextForSource3()
        {
            const string sourcePath = "IntProperty";
            var targetObj = new BindingSourceEventNotifierModel();
            var builder = new BindingBuilder();
            builder.Bind(targetObj, () => model => model.ObjectProperty).To(sourcePath);

            IList<Func<IDataContext, IBindingSource>> sources = builder.GetData(BindingBuilderConstants.Sources);
            IBindingSource source = sources.Single().Invoke(builder);
            BindingParserTest.BindingSourceShouldBeValidDataContext(targetObj, source, sourcePath);
            var sourceObj = new BindingSourceModel();
            BindingServiceProvider.ContextManager.GetBindingContext(targetObj).Value = sourceObj;
            BindingParserTest.BindingSourceShouldBeValidDataContext(targetObj, source, sourcePath);
        }

        [TestMethod]
        public void BuilderShouldUseTargetBindingContextForSource4()
        {
            const string sourcePath = "IntProperty";
            var targetObj = new BindingSourceEventNotifierModel();
            var builder = new BindingBuilder();
            builder.Bind(targetObj, () => model => model.ObjectProperty).To<BindingSourceModel>(() => model => model.IntProperty);

            IList<Func<IDataContext, IBindingSource>> sources = builder.GetData(BindingBuilderConstants.Sources);
            IBindingSource source = sources.Single().Invoke(builder);
            BindingParserTest.BindingSourceShouldBeValidDataContext(targetObj, source, sourcePath);
            var sourceObj = new BindingSourceModel();
            BindingServiceProvider.ContextManager.GetBindingContext(targetObj).Value = sourceObj;
            BindingParserTest.BindingSourceShouldBeValidDataContext(targetObj, source, sourcePath);
        }

        [TestMethod]
        public void BuilderShouldUseSourceObject1()
        {
            const string targetPath = "Text";
            const string sourcePath = "IntProperty";
            var targetObj = new object();
            var sourceObj = new BindingSourceModel();
            var builder = new BindingBuilder();
            builder.Bind(targetObj, targetPath).To(sourceObj, sourcePath);

            IList<Func<IDataContext, IBindingSource>> sources = builder.GetData(BindingBuilderConstants.Sources);
            IBindingSource source = sources.Single().Invoke(builder);
            BindingParserTest.BindingSourceShouldBeValid(source, sourcePath, sourceObj);
        }

        [TestMethod]
        public void BuilderShouldUseSourceObject2()
        {
            const string targetPath = "Text";
            const string sourcePath = "IntProperty";
            var targetObj = new object();
            var sourceObj = new BindingSourceModel();
            var builder = new BindingBuilder();
            builder.Bind(targetObj, targetPath).To(sourceObj, () => model => model.IntProperty);

            IList<Func<IDataContext, IBindingSource>> sources = builder.GetData(BindingBuilderConstants.Sources);
            IBindingSource source = sources.Single().Invoke(builder);
            BindingParserTest.BindingSourceShouldBeValid(source, sourcePath, sourceObj);
        }

        [TestMethod]
        public void BuilderShouldUseSourceObject3()
        {
            const string sourcePath = "IntProperty";
            var targetObj = new BindingSourceEventNotifierModel();
            var sourceObj = new BindingSourceModel();
            var builder = new BindingBuilder();
            builder.Bind(targetObj, () => model => model.ObjectProperty).To(sourceObj, sourcePath);

            IList<Func<IDataContext, IBindingSource>> sources = builder.GetData(BindingBuilderConstants.Sources);
            IBindingSource source = sources.Single().Invoke(builder);
            BindingParserTest.BindingSourceShouldBeValid(source, sourcePath, sourceObj);
        }

        [TestMethod]
        public void BuilderShouldUseSourceObject4()
        {
            const string sourcePath = "IntProperty";
            var targetObj = new BindingSourceEventNotifierModel();
            var sourceObj = new BindingSourceModel();
            var builder = new BindingBuilder();
            builder.Bind(targetObj, () => model => model.ObjectProperty).To(sourceObj, () => model => model.IntProperty);

            IList<Func<IDataContext, IBindingSource>> sources = builder.GetData(BindingBuilderConstants.Sources);
            IBindingSource source = sources.Single().Invoke(builder);
            BindingParserTest.BindingSourceShouldBeValid(source, sourcePath, sourceObj);
        }

        [TestMethod]
        public void BuilderShouldUseSelfAsSourceObject1()
        {
            const string targetPath = "Text";
            const string sourcePath = "IntProperty";
            var targetObj = new BindingSourceModel();
            var builder = new BindingBuilder();
            builder.Bind(targetObj, targetPath).ToSelf(() => model => model.IntProperty);

            IList<Func<IDataContext, IBindingSource>> sources = builder.GetData(BindingBuilderConstants.Sources);
            IBindingSource source = sources.Single().Invoke(builder);
            BindingParserTest.BindingSourceShouldBeValid(source, sourcePath, targetObj);
        }

        [TestMethod]
        public void BuilderShouldUseSelfAsSourceObject2()
        {
            const string targetPath = "Text";
            const string sourcePath = "IntProperty";
            var targetObj = new BindingSourceModel();
            var builder = new BindingBuilder();
            builder.Bind(targetObj, targetPath).ToSelf(sourcePath);

            IList<Func<IDataContext, IBindingSource>> sources = builder.GetData(BindingBuilderConstants.Sources);
            IBindingSource source = sources.Single().Invoke(builder);
            BindingParserTest.BindingSourceShouldBeValid(source, sourcePath, targetObj);
        }

        [TestMethod]
        public void BuilderShouldUseBindingMode()
        {
            var modes = new Dictionary<IBindingBehavior, Action<IBindingModeSyntax>>
            {
                {new TwoWayBindingMode(), syntax => syntax.TwoWay()},
                {new OneWayBindingMode(), syntax => syntax.OneWay()},
                {new OneTimeBindingMode(), syntax => syntax.OneTime()},
                {new OneWayToSourceBindingMode(), syntax => syntax.OneWayToSource() },
                {NoneBindingMode.Instance, syntax => syntax.NoneMode()},
            };

            foreach (var action in modes)
            {
                var builder = new BindingBuilder();
                action.Value(builder.Bind(new object(), "test").To("test"));
                var behaviors = builder.GetData(BindingBuilderConstants.Behaviors);
                behaviors.Single().ShouldBeType(action.Key.GetType());
            }
        }

        [TestMethod]
        public void BuilderShouldUseCustomBehavior()
        {
            var mock = new BindingBehaviorMock();
            var builder = new BindingBuilder();
            builder.Bind(new object(), "test").To("test").WithBehavior(mock);
            builder.GetData(BindingBuilderConstants.Behaviors).Single().ShouldEqual(mock);
        }

        [TestMethod]
        public void BuilderShouldUseValidatesOnExceptionsBehavior()
        {
            var builder = new BindingBuilder();
            builder.Bind(new object(), "test").To("test").ValidatesOnExceptions();
            builder.GetData(BindingBuilderConstants.Behaviors).Single().ShouldBeType<ValidatesOnExceptionsBehavior>();
        }

        [TestMethod]
        public void BuilderShouldUseValidatesOnNotifyDataErrorsBehavior()
        {
            var builder = new BindingBuilder();
            builder.Bind(new object(), "test").To("test").ValidatesOnNotifyDataErrors();
            builder.GetData(BindingBuilderConstants.Behaviors).Single().ShouldBeType<ValidatesOnNotifyDataErrorsBehavior>();
        }

        [TestMethod]
        public void BuilderShouldUseValidateBehaviors()
        {
            var builder = new BindingBuilder();
            builder.Bind(new object(), "test").To("test").Validate();
            var behaviors = builder.GetData(BindingBuilderConstants.Behaviors);
            behaviors.Count.ShouldEqual(2);
            behaviors.OfType<ValidatesOnExceptionsBehavior>().Single().ShouldNotBeNull();
            behaviors.OfType<ValidatesOnNotifyDataErrorsBehavior>().Single().ShouldNotBeNull();
        }

        [TestMethod]
        public void BuilderShouldUseConverter1()
        {
            var converter = new ValueConverterCoreMock();
            var builder = new BindingBuilder();
            builder.Bind(new object(), "test").To("test").WithConverter(converter);
            builder.GetData(BindingBuilderConstants.Converter).Invoke(builder).ShouldEqual(converter);
        }

        [TestMethod]
        public void BuilderShouldUseConverter2()
        {
            Func<IDataContext, IBindingValueConverter> converter = context => null;
            var builder = new BindingBuilder();
            builder.Bind(new object(), "test").To("test").WithConverter(converter);
            builder.GetData(BindingBuilderConstants.Converter).ShouldEqual(converter);
        }

        [TestMethod]
        public void BuilderShouldUseFallback1()
        {
            var fallback = new object();
            var builder = new BindingBuilder();
            builder.Bind(new object(), "test").To("test").WithFallback(fallback);
            builder.GetData(BindingBuilderConstants.Fallback).Invoke(builder).ShouldEqual(fallback);
        }

        [TestMethod]
        public void BuilderShouldUseFallback2()
        {
            Func<IDataContext, object> fallback = context => null;
            var builder = new BindingBuilder();
            builder.Bind(new object(), "test").To("test").WithFallback(fallback);
            builder.GetData(BindingBuilderConstants.Fallback).ShouldEqual(fallback);
        }

        [TestMethod]
        public void BuilderShouldUseDelayBehaviorSource()
        {
            var builder = new BindingBuilder();
            builder.Bind(new object(), "test").To("test").WithDelay(10);
            var behavior = builder.GetData(BindingBuilderConstants.Behaviors).OfType<DelayBindingBehavior>().Single();
            behavior.Delay.ShouldEqual(10);
            behavior.IsTarget.ShouldBeFalse();
        }

        [TestMethod]
        public void BuilderShouldUseDelayBehaviorTarget()
        {
            var builder = new BindingBuilder();
            builder.Bind(new object(), "test").To("test").WithDelay(10, true);
            var behavior = builder.GetData(BindingBuilderConstants.Behaviors).OfType<DelayBindingBehavior>().Single();
            behavior.Delay.ShouldEqual(10);
            behavior.IsTarget.ShouldBeTrue();
        }

        [TestMethod]
        public void BuilderShouldUseConverterCulture1()
        {
            var culture = CultureInfo.InvariantCulture;
            var builder = new BindingBuilder();
            builder.Bind(new object(), "test").To("test").WithConverterCulture(culture);
            builder.GetData(BindingBuilderConstants.ConverterCulture).Invoke(builder).ShouldEqual(culture);
        }

        [TestMethod]
        public void BuilderShouldUseConverterCulture2()
        {
            Func<IDataContext, CultureInfo> culture = context => null;
            var builder = new BindingBuilder();
            builder.Bind(new object(), "test").To("test").WithConverterCulture(culture);
            builder.GetData(BindingBuilderConstants.ConverterCulture).ShouldEqual(culture);
        }

        [TestMethod]
        public void BuilderShouldUseCommandParameter1()
        {
            var parameter = new object();
            var builder = new BindingBuilder();
            builder.Bind(new object(), "test").To("test").WithCommandParameter(parameter);
            builder.GetData(BindingBuilderConstants.CommandParameter).Invoke(builder).ShouldEqual(parameter);
        }

        [TestMethod]
        public void BuilderShouldUseCommandParameter2()
        {
            Func<IDataContext, object> parameter = context => null;
            var builder = new BindingBuilder();
            builder.Bind(new object(), "test").To("test").WithCommandParameter(parameter);
            builder.GetData(BindingBuilderConstants.CommandParameter).ShouldEqual(parameter);
        }

        [TestMethod]
        public void BuilderShouldUseConverterParameter1()
        {
            var parameter = new object();
            var builder = new BindingBuilder();
            builder.Bind(new object(), "test").To("test").WithConverterParameter(parameter);
            builder.GetData(BindingBuilderConstants.ConverterParameter).Invoke(builder).ShouldEqual(parameter);
        }

        [TestMethod]
        public void BuilderShouldUseConverterParameter2()
        {
            Func<IDataContext, object> parameter = context => null;
            var builder = new BindingBuilder();
            builder.Bind(new object(), "test").To("test").WithConverterParameter(parameter);
            builder.GetData(BindingBuilderConstants.ConverterParameter).ShouldEqual(parameter);
        }

        [TestMethod]
        public void BuilderShouldUseTargetNullValue()
        {
            var nullValue = new object();
            var builder = new BindingBuilder();
            builder.Bind(new object(), "test").To("test").WithTargetNullValue(nullValue);
            builder.GetData(BindingBuilderConstants.TargetNullValue).ShouldEqual(nullValue);
        }

        [TestMethod]
        public void BuilderShouldUseLambdaExpression1()
        {
            var builder = new BindingBuilder();
            builder.Bind(new object(), "empty")
                .To<BindingSourceModel>(() => model => model.IntProperty + 100 + model.NestedModel.IntProperty + int.Parse(model.NestedModel["1"]));

            var expression = builder.GetData(BindingBuilderConstants.MultiExpression);
            expression.Invoke(builder, new object[] { 10, 10, "10" }).ShouldEqual(130);
            expression.Invoke(builder, new object[] { -10, 10, "-100" }).ShouldEqual(0);

            var list = builder.GetData(BindingBuilderConstants.Sources);
            list.Count.ShouldEqual(3);
            list[0].Invoke(builder).Path.Path.ShouldEqual(GetMemberPath<BindingSourceModel>(model => model.IntProperty));
            list[1].Invoke(builder).Path.Path.ShouldEqual(GetMemberPath<BindingSourceModel>(model => model.NestedModel.IntProperty));
            list[2].Invoke(builder).Path.Path.ShouldEqual(GetMemberPath<BindingSourceModel>(model => model.NestedModel["1"]));
        }

        [TestMethod]
        public void BuilderShouldUseLambdaExpression2()
        {
            var builder = new BindingBuilder();
            builder.Bind(new object(), "empty")
                .To<BindingSourceModel>(() => model => model.StringProperty.OfType<char>().Count(c => c == '1') + ((BindingSourceModel)model.ObjectProperty).IntProperty);

            var expression = builder.GetData(BindingBuilderConstants.MultiExpression);
            expression.Invoke(builder, new object[] { "1", 10 }).ShouldEqual(11);
            expression.Invoke(builder, new object[] { "", 0 }).ShouldEqual(0);

            var list = builder.GetData(BindingBuilderConstants.Sources);
            list.Count.ShouldEqual(2);
            list[0].Invoke(builder).Path.Path.ShouldEqual(GetMemberPath<BindingSourceModel>(model => model.StringProperty));
            list[1].Invoke(builder).Path.Path.ShouldEqual(GetMemberPath<BindingSourceModel>(model => ((BindingSourceModel)model.ObjectProperty).IntProperty));
        }

        [TestMethod]
        public void BuilderShouldUseLambdaExpression3()
        {
            var builder = new BindingBuilder();
            builder.Bind(new object(), "empty")
                .To<BindingSourceModel>(() => model => model.StringProperty.OfType<char>().Select(s => s == null ? 10 + 4 : 3 + 10).FirstOrDefault() == 0 ? false : true || true);

            var expression = builder.GetData(BindingBuilderConstants.MultiExpression);
            expression.Invoke(builder, new object[] { "1" }).ShouldEqual("1".OfType<char>().Select(s => s == null ? 10 + 4 : 3 + 10).FirstOrDefault() == 0 ? false : true || true);
            expression.Invoke(builder, new object[] { "2" }).ShouldEqual("2".OfType<char>().Select(s => s == null ? 10 + 4 : 3 + 10).FirstOrDefault() == 0 ? false : true || true);

            var list = builder.GetData(BindingBuilderConstants.Sources);
            list.Count.ShouldEqual(1);
            list[0].Invoke(builder).Path.Path.ShouldEqual(GetMemberPath<BindingSourceModel>(model => model.StringProperty));
        }

        [TestMethod]
        public void BuilderShouldUseEventArgs1()
        {
            var builder = new BindingBuilder();
            builder.Bind(new object(), "empty").To<BindingSourceModel>(() => model => BindingSyntaxEx.EventArgs<EventArgs>());

            builder.Add(BindingConstants.CurrentEventArgs, EventArgs.Empty);
            var expression = builder.GetData(BindingBuilderConstants.MultiExpression);
            expression(builder, Empty.Array<object>()).ShouldEqual(EventArgs.Empty);
        }

        [TestMethod]
        public void BuilderShouldUseEventArgs2()
        {
            var builder = new BindingBuilder();
            builder.Bind(new object(), "empty").To<BindingSourceModel>(() => model => BindingSyntaxEx.EventArgs<EventArgs>().GetType().Name);

            builder.Add(BindingConstants.CurrentEventArgs, EventArgs.Empty);
            var expression = builder.GetData(BindingBuilderConstants.MultiExpression);
            expression(builder, Empty.Array<object>()).ShouldEqual(EventArgs.Empty.GetType().Name);
        }

        [TestMethod]
        public void BuilderShouldUseEventArgs3()
        {
            var builder = new BindingBuilder();
            builder.Bind(new object(), "empty").To<BindingSourceModel>(() => model => BindingSyntaxEx.EventArgs<BindingSourceModel>().ObjectProperty);

            var sourceModel = new BindingSourceModel { ObjectProperty = "test" };
            builder.Add(BindingConstants.CurrentEventArgs, sourceModel);
            var expression = builder.GetData(BindingBuilderConstants.MultiExpression);
            expression(builder, Empty.Array<object>()).ShouldEqual(sourceModel.ObjectProperty);
        }

        [TestMethod]
        public void BuilderShouldUseSelfExpression1()
        {
            var builder = new BindingBuilder();
            var sourceModel = new BindingSourceModel();
            builder.Bind(sourceModel, "empty").To<BindingSourceModel>(() => model => BindingSyntaxEx.Self<BindingSourceModel>());

            var source = builder.GetData(BindingBuilderConstants.Sources).Single().Invoke(builder);
            builder.GetData(BindingBuilderConstants.MultiExpression).ShouldBeNull();
            source.Path.IsEmpty.ShouldBeTrue();
            source.GetSource(true).ShouldEqual(sourceModel);
            var pathMembers = source.GetPathMembers(true);
            pathMembers.LastMember.GetValue(pathMembers.PenultimateValue, null).ShouldEqual(sourceModel);
        }

        [TestMethod]
        public void BuilderShouldUseSelfExpression2()
        {
            var builder = new BindingBuilder();
            var sourceModel = new BindingSourceModel { ObjectProperty = "test" };
            builder.Bind(sourceModel, "empty").To<BindingSourceModel>(() => model => BindingSyntaxEx.Self<BindingSourceModel>().ObjectProperty);

            var source = builder.GetData(BindingBuilderConstants.Sources).Single().Invoke(builder);
            builder.GetData(BindingBuilderConstants.MultiExpression).ShouldBeNull();
            source.Path.Path.ShouldEqual(GetMemberPath(sourceModel, model => model.ObjectProperty));
            source.GetSource(true).ShouldEqual(sourceModel);
            var pathMembers = source.GetPathMembers(true);
            pathMembers.LastMember.GetValue(pathMembers.PenultimateValue, null).ShouldEqual(sourceModel.ObjectProperty);
        }

        [TestMethod]
        public void BuilderShouldUseResource1()
        {
            const string key = "key";
            var builder = new BindingBuilder();
            var sourceModel = new BindingSourceModel();
            BindingServiceProvider.ResourceResolver.AddObject(key, sourceModel);
            builder.Bind(sourceModel, "empty").To<BindingSourceModel>(() => model => BindingSyntaxEx.Resource<BindingSourceModel>(key));

            var source = builder.GetData(BindingBuilderConstants.Sources).Single().Invoke(builder);
            builder.GetData(BindingBuilderConstants.MultiExpression).ShouldBeNull();
            source.Path.IsEmpty.ShouldBeTrue();
            source.GetSource(true).ShouldEqual(sourceModel);
            var pathMembers = source.GetPathMembers(true);
            pathMembers.LastMember.GetValue(pathMembers.PenultimateValue, null).ShouldEqual(sourceModel);
        }

        [TestMethod]
        public void BuilderShouldUseResource2()
        {
            const string key = "key";
            var builder = new BindingBuilder();
            var sourceModel = new BindingSourceModel { ObjectProperty = "test" };
            BindingServiceProvider.ResourceResolver.AddObject(key, sourceModel);
            builder.Bind(sourceModel, "empty").To<BindingSourceModel>(() => model => BindingSyntaxEx.Self<BindingSourceModel>().ObjectProperty);

            var source = builder.GetData(BindingBuilderConstants.Sources).Single().Invoke(builder);
            builder.GetData(BindingBuilderConstants.MultiExpression).ShouldBeNull();
            source.Path.Path.ShouldEqual(GetMemberPath(sourceModel, model => model.ObjectProperty));
            source.GetSource(true).ShouldEqual(sourceModel);
            var pathMembers = source.GetPathMembers(true);
            pathMembers.LastMember.GetValue(pathMembers.PenultimateValue, null).ShouldEqual(sourceModel.ObjectProperty);
        }

        [TestMethod]
        public void BuilderShouldUseGetErrorsMethod1()
        {
            var builder = new BindingBuilder();
            var sourceModel = new BindingSourceModel();
            builder.Bind(sourceModel, "empty").To(sourceModel, () => model => BindingSyntaxEx.GetErrors());

            var source = builder.GetData(BindingBuilderConstants.Sources).Single().Invoke(builder);
            source.Path.IsEmpty.ShouldBeTrue();
            source.GetSource(true).ShouldEqual(sourceModel);

            var behavior = builder.GetOrAddBehaviors().OfType<NotifyDataErrorsAggregatorBehavior>().Single();
            behavior.ErrorPaths.IsNullOrEmpty().ShouldBeTrue();
            builder.AddOrUpdate(BindingConstants.Binding, new DataBindingMock { Behaviors = new[] { behavior } });

            var expression = builder.GetData(BindingBuilderConstants.MultiExpression);
            behavior.Errors = new List<object> { "test" };
            expression(builder, new object[] { sourceModel }).ShouldEqual(behavior.Errors);
        }

        [TestMethod]
        public void BuilderShouldUseGetErrorsMethod2()
        {
            var builder = new BindingBuilder();
            var sourceModel = new BindingSourceModel();
            builder.Bind(sourceModel, "empty").To(sourceModel, () => model => BindingSyntaxEx.GetErrors("1", "2"));

            var source = builder.GetData(BindingBuilderConstants.Sources).Single().Invoke(builder);
            source.Path.IsEmpty.ShouldBeTrue();
            source.GetSource(true).ShouldEqual(sourceModel);

            var behavior = builder.GetOrAddBehaviors().OfType<NotifyDataErrorsAggregatorBehavior>().Single();
            behavior.ErrorPaths.SequenceEqual(new[] { "1", "2" }).ShouldBeTrue();
            builder.AddOrUpdate(BindingConstants.Binding, new DataBindingMock { Behaviors = new[] { behavior } });

            var expression = builder.GetData(BindingBuilderConstants.MultiExpression);
            behavior.Errors = new List<object> { "test" };
            expression(builder, new object[] { sourceModel }).ShouldEqual(behavior.Errors);
        }

        [TestMethod]
        public void BuilderShouldUseGetErrorsMethod3()
        {
            var builder = new BindingBuilder();
            var sourceModel = new BindingSourceModel();
            builder.Bind(sourceModel, "empty").To(sourceModel, () => model => BindingSyntaxEx.GetErrors("1", "2", model.ObjectProperty));

            var source = builder.GetData(BindingBuilderConstants.Sources).Single().Invoke(builder);
            source.Path.Path.ShouldEqual(GetMemberPath(sourceModel, model => model.ObjectProperty));
            source.GetSource(true).ShouldEqual(sourceModel);

            var behavior = builder.GetOrAddBehaviors().OfType<NotifyDataErrorsAggregatorBehavior>().Single();
            behavior.ErrorPaths.SequenceEqual(new[] { "1", "2" }).ShouldBeTrue();
            builder.AddOrUpdate(BindingConstants.Binding, new DataBindingMock { Behaviors = new[] { behavior } });

            var expression = builder.GetData(BindingBuilderConstants.MultiExpression);
            behavior.Errors = new List<object> { "test" };
            expression(builder, new object[] { sourceModel }).ShouldEqual(behavior.Errors);
        }

        [TestMethod]
        public void BuilderShouldUseGetErrorsMethod4()
        {
            var builder = new BindingBuilder();
            var sourceModel = new BindingSourceModel();
            builder.Bind(sourceModel, "empty").To(sourceModel, () => model => BindingSyntaxEx.GetErrors("1", "2").Concat(BindingSyntaxEx.GetErrors(model.ObjectProperty)));


            var sources = builder.GetData(BindingBuilderConstants.Sources).Select(func => func(builder)).ToArray();
            sources[0].GetSource(true).ShouldEqual(sourceModel);
            sources[1].GetSource(true).ShouldEqual(sourceModel);
            if (sources[0].Path.IsEmpty)
                sources[1].Path.Path.ShouldEqual(GetMemberPath(sourceModel, model => model.ObjectProperty));
            else
                sources[0].Path.Path.ShouldEqual(GetMemberPath(sourceModel, model => model.ObjectProperty));

            var behaviors = builder.GetOrAddBehaviors().OfType<NotifyDataErrorsAggregatorBehavior>().ToArray();
            builder.AddOrUpdate(BindingConstants.Binding, new DataBindingMock { Behaviors = behaviors });
            var behavior = behaviors.Single(b => !b.ErrorPaths.IsNullOrEmpty());
            behavior.ErrorPaths.SequenceEqual(new[] { "1", "2" }).ShouldBeTrue();

            behaviors.ForEach(aggregatorBehavior => aggregatorBehavior.Errors = new[] { "1" });
            var expression = builder.GetData(BindingBuilderConstants.MultiExpression);
            var errors = (IEnumerable<object>)expression(builder, new object[] { sourceModel, sourceModel });
            errors.SequenceEqual(behaviors[0].Errors.Concat(behaviors[1].Errors)).ShouldBeTrue();
        }

        [TestMethod]
        public void BuilderShouldUseRelativeSource1()
        {
            var builder = new BindingBuilder();
            var targetObj = new BindingSourceModel { ObjectProperty = "test" };
            var relativeObj = new BindingSourceModel();
            bool isInvoked = false;
            IEventListener eventListener = null;
            var memberMock = new BindingMemberInfoMock
            {
                TryObserveMember = (o, listener) =>
                {
                    eventListener = listener;
                    return null;
                }
            };

            var treeManagerMock = new VisualTreeManagerMock
            {
                FindRelativeSource = (o, s, arg3) =>
                {
                    o.ShouldEqual(targetObj);
                    s.ShouldEqual(typeof(BindingSourceModel).AssemblyQualifiedName);
                    arg3.ShouldEqual(1u);
                    isInvoked = true;
                    return relativeObj;
                },
                GetRootMember = type => memberMock
            };
            BindingServiceProvider.VisualTreeManager = treeManagerMock;

            builder.Bind(targetObj, "empty").To<BindingSourceModel>(() => model => BindingSyntaxEx.Relative<BindingSourceModel>());
            var source = builder.GetData(BindingBuilderConstants.Sources).Single().Invoke(builder);
            builder.GetData(BindingBuilderConstants.MultiExpression).ShouldBeNull();
            source.Path.Path.IsEmpty().ShouldBeTrue();
            source.GetSource(true).ShouldEqual(relativeObj);
            var pathMembers = source.GetPathMembers(true);
            pathMembers.LastMember.GetValue(pathMembers.PenultimateValue, null).ShouldEqual(relativeObj);
            isInvoked.ShouldBeTrue();

            isInvoked = false;
            eventListener.ShouldNotBeNull();
            eventListener.Handle(this, EventArgs.Empty);
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void BuilderShouldUseRelativeSource2()
        {
            const uint level = 10u;
            var builder = new BindingBuilder();
            var targetObj = new BindingSourceModel { ObjectProperty = "test" };
            var relativeObj = new BindingSourceModel();
            bool isInvoked = false;
            IEventListener eventListener = null;
            var memberMock = new BindingMemberInfoMock
            {
                TryObserveMember = (o, listener) =>
                {
                    eventListener = listener;
                    return null;
                }
            };

            var treeManagerMock = new VisualTreeManagerMock
            {
                FindRelativeSource = (o, s, arg3) =>
                {
                    o.ShouldEqual(targetObj);
                    s.ShouldEqual(typeof(BindingSourceModel).AssemblyQualifiedName);
                    arg3.ShouldEqual(level);
                    isInvoked = true;
                    return relativeObj;
                },
                GetRootMember = type => memberMock
            };
            BindingServiceProvider.VisualTreeManager = treeManagerMock;

            builder.Bind(targetObj, "empty").To<BindingSourceModel>(() => model => BindingSyntaxEx.Relative<BindingSourceModel>(level).ObjectProperty);
            var source = builder.GetData(BindingBuilderConstants.Sources).Single().Invoke(builder);
            builder.GetData(BindingBuilderConstants.MultiExpression).ShouldBeNull();
            source.Path.Path.ShouldEqual(GetMemberPath(targetObj, model => model.ObjectProperty));
            source.GetSource(true).ShouldEqual(relativeObj);
            var pathMembers = source.GetPathMembers(true);
            pathMembers.LastMember.GetValue(pathMembers.PenultimateValue, null).ShouldEqual(relativeObj.ObjectProperty);
            isInvoked.ShouldBeTrue();

            isInvoked = false;
            eventListener.ShouldNotBeNull();
            eventListener.Handle(this, EventArgs.Empty);
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void BuilderShouldUseElementSource1()
        {
            const string name = "name";
            var builder = new BindingBuilder();
            var targetObj = new BindingSourceModel { ObjectProperty = "test" };
            var element = new BindingSourceModel();
            bool isInvoked = false;
            IEventListener eventListener = null;
            var memberMock = new BindingMemberInfoMock
            {
                TryObserveMember = (o, listener) =>
                {
                    eventListener = listener;
                    return null;
                }
            };

            var treeManagerMock = new VisualTreeManagerMock
            {
                FindByName = (o, s) =>
                {
                    o.ShouldEqual(targetObj);
                    s.ShouldEqual(name);
                    isInvoked = true;
                    return element;
                },
                GetRootMember = type => memberMock
            };
            BindingServiceProvider.VisualTreeManager = treeManagerMock;

            builder.Bind(targetObj, "empty").To<BindingSourceModel>(() => model => BindingSyntaxEx.Element<BindingSourceModel>(name));
            var source = builder.GetData(BindingBuilderConstants.Sources).Single().Invoke(builder);
            builder.GetData(BindingBuilderConstants.MultiExpression).ShouldBeNull();
            source.Path.Path.IsEmpty().ShouldBeTrue();
            source.GetSource(true).ShouldEqual(element);
            var pathMembers = source.GetPathMembers(true);
            pathMembers.LastMember.GetValue(pathMembers.PenultimateValue, null).ShouldEqual(element);
            isInvoked.ShouldBeTrue();

            isInvoked = false;
            eventListener.ShouldNotBeNull();
            eventListener.Handle(this, EventArgs.Empty);
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void BuilderShouldUseElementSource2()
        {
            const string name = "name";
            var builder = new BindingBuilder();
            var targetObj = new BindingSourceModel { ObjectProperty = "test" };
            var relativeObj = new BindingSourceModel();
            bool isInvoked = false;
            IEventListener eventListener = null;
            var memberMock = new BindingMemberInfoMock
            {
                TryObserveMember = (o, listener) =>
                {
                    eventListener = listener;
                    return null;
                }
            };

            var treeManagerMock = new VisualTreeManagerMock
            {
                FindByName = (o, s) =>
                {
                    o.ShouldEqual(targetObj);
                    s.ShouldEqual(name);
                    isInvoked = true;
                    return relativeObj;
                },
                GetRootMember = type => memberMock
            };
            BindingServiceProvider.VisualTreeManager = treeManagerMock;

            builder.Bind(targetObj, "empty").To<BindingSourceModel>(() => model => BindingSyntaxEx.Element<BindingSourceModel>(name).ObjectProperty);
            var source = builder.GetData(BindingBuilderConstants.Sources).Single().Invoke(builder);
            builder.GetData(BindingBuilderConstants.MultiExpression).ShouldBeNull();
            source.Path.Path.ShouldEqual(GetMemberPath(targetObj, model => model.ObjectProperty));
            source.GetSource(true).ShouldEqual(relativeObj);
            var pathMembers = source.GetPathMembers(true);
            pathMembers.LastMember.GetValue(pathMembers.PenultimateValue, null).ShouldEqual(relativeObj.ObjectProperty);
            isInvoked.ShouldBeTrue();

            isInvoked = false;
            eventListener.ShouldNotBeNull();
            eventListener.Handle(this, EventArgs.Empty);
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void BuilderShouldUseResourceMethod1()
        {
            const string key = "key";
            var builder = new BindingBuilder();
            var sourceModel = new BindingSourceModel();
            var result = new BindingSourceModel();
            BindingServiceProvider.ResourceResolver.AddMethod<string, BindingSourceModel>(key, (s, context) =>
            {
                context.ShouldEqual(builder);
                return result;
            });
            builder.Bind(sourceModel, "empty").To<BindingSourceModel>(() => model => BindingSyntaxEx.ResourceMethod<BindingSourceModel>(key, key));

            var sources = builder.GetData(BindingBuilderConstants.Sources);
            sources.Count.ShouldEqual(1);
            sources[0].Invoke(builder).Path.Path.ShouldEqual(string.Empty);
            var expression = builder.GetData(BindingBuilderConstants.MultiExpression);
            expression(builder, Empty.Array<object>()).ShouldEqual(result);
        }

        [TestMethod]
        public void BuilderShouldUseResourceMethod2()
        {
            const string key = "key";
            var builder = new BindingBuilder();
            var sourceModel = new BindingSourceModel();
            var result = new BindingSourceModel { ObjectProperty = "Test" };
            BindingServiceProvider.ResourceResolver.AddMethod<string, object, BindingSourceModel>(key, (s1, s2, context) =>
            {
                s1.ShouldEqual(key);
                s2.ShouldEqual(builder);
                context.ShouldEqual(builder);
                return result;
            });
            builder.Bind(sourceModel, "empty").To<BindingSourceModel>(() => model => BindingSyntaxEx.ResourceMethod<BindingSourceModel>(key, key, builder).ObjectProperty);

            var sources = builder.GetData(BindingBuilderConstants.Sources);
            sources.Count.ShouldEqual(1);
            sources[0].Invoke(builder).Path.Path.ShouldEqual(string.Empty);
            var expression = builder.GetData(BindingBuilderConstants.MultiExpression);
            expression(builder, Empty.Array<object>()).ShouldEqual(result.ObjectProperty);
        }

        [TestMethod]
        public void BuilderShouldUseBindingContextExtension1()
        {
            const string targetPath = "Text";
            var targetObj = new object();
            var builder = new BindingBuilder();
            builder.Bind(targetObj, targetPath).To<BindingSourceModel>(() => model => BindingSyntaxEx.DataContext<BindingSourceModel>());

            IList<Func<IDataContext, IBindingSource>> sources = builder.GetData(BindingBuilderConstants.Sources);
            IBindingSource source = sources.Single().Invoke(builder);
            BindingParserTest.BindingSourceShouldBeValidDataContext(targetObj, source, string.Empty);
            var sourceObj = new BindingSourceModel();
            BindingServiceProvider.ContextManager.GetBindingContext(targetObj).Value = sourceObj;
            BindingParserTest.BindingSourceShouldBeValidDataContext(targetObj, source, string.Empty);
        }

        [TestMethod]
        public void BuilderShouldUseBindingContextExtension2()
        {
            const string targetPath = "Text";
            const string sourcePath = "IntProperty";
            var targetObj = new object();
            var builder = new BindingBuilder();
            builder.Bind(targetObj, targetPath).To<BindingSourceModel>(() => model => BindingSyntaxEx.DataContext<BindingSourceModel>().IntProperty);

            IList<Func<IDataContext, IBindingSource>> sources = builder.GetData(BindingBuilderConstants.Sources);
            IBindingSource source = sources.Single().Invoke(builder);
            BindingParserTest.BindingSourceShouldBeValidDataContext(targetObj, source, sourcePath);
            var sourceObj = new BindingSourceModel();
            BindingServiceProvider.ContextManager.GetBindingContext(targetObj).Value = sourceObj;
            BindingParserTest.BindingSourceShouldBeValidDataContext(targetObj, source, sourcePath);
        }

        [TestMethod]
        public void BuilderShouldUseBindingContextExtension3()
        {
            const string targetPath = "Text";
            var targetObj = new object();
            var builder = new BindingBuilder();
            builder.Bind(targetObj, targetPath).To<BindingSourceModel>(() => model => BindingSyntaxEx.Self<object>().DataContext<BindingSourceModel>());

            IList<Func<IDataContext, IBindingSource>> sources = builder.GetData(BindingBuilderConstants.Sources);
            IBindingSource source = sources.Single().Invoke(builder);
            source.Path.Path.ShouldEqual(AttachedMemberConstants.DataContext);
        }

        [TestMethod]
        public void BuilderShouldUseBindingContextExtension4()
        {
            const string targetPath = "Text";
            const string sourcePath = "IntProperty";
            var targetObj = new object();
            var builder = new BindingBuilder();
            builder.Bind(targetObj, targetPath).To<BindingSourceModel>(() => model => BindingSyntaxEx.Self<object>().DataContext<BindingSourceModel>().IntProperty);

            IList<Func<IDataContext, IBindingSource>> sources = builder.GetData(BindingBuilderConstants.Sources);
            IBindingSource source = sources.Single().Invoke(builder);
            source.Path.Path.ShouldEqual(AttachedMemberConstants.DataContext + "." + sourcePath);
        }

        [TestMethod]
        public void BuilderShouldUseDynamicMember1()
        {
            const string targetPath = "Text";
            const string sourcePath = "IntProperty";
            var targetObj = new object();
            var builder = new BindingBuilder();
            builder.Bind(targetObj, targetPath).To<BindingSourceModel>(() => model => model.Member<int>(sourcePath));

            IList<Func<IDataContext, IBindingSource>> sources = builder.GetData(BindingBuilderConstants.Sources);
            IBindingSource source = sources.Single().Invoke(builder);
            BindingParserTest.BindingSourceShouldBeValidDataContext(targetObj, source, sourcePath);
            var sourceObj = new BindingSourceModel();
            BindingServiceProvider.ContextManager.GetBindingContext(targetObj).Value = sourceObj;
            BindingParserTest.BindingSourceShouldBeValidDataContext(targetObj, source, sourcePath);
        }

        [TestMethod]
        public void BuilderShouldUseDynamicMember2()
        {
            const string targetPath = "Text";
            var targetObj = new object();
            var builder = new BindingBuilder();
            var src = new BindingSourceModel { ObjectProperty = new BindingSourceModel { StringProperty = "test" } };
            builder.Bind(targetObj, targetPath).To<BindingSourceModel>(() => model => model.GetObjectProperty().Member<string>("StringProperty"));

            IList<Func<IDataContext, IBindingSource>> sources = builder.GetData(BindingBuilderConstants.Sources);
            IBindingSource source = sources.Single().Invoke(builder);
            BindingParserTest.BindingSourceShouldBeValidDataContext(targetObj, source, string.Empty);
            var sourceObj = new BindingSourceModel();
            BindingServiceProvider.ContextManager.GetBindingContext(targetObj).Value = sourceObj;
            BindingParserTest.BindingSourceShouldBeValidDataContext(targetObj, source, string.Empty);

            var data = builder.GetData(BindingBuilderConstants.MultiExpression);
            data.Invoke(builder, new[] { src }).ShouldEqual(((BindingSourceModel)src.GetObjectProperty()).StringProperty);
        }

        [TestMethod]
        public void BuilderShouldUseDynamicMember3()
        {
            const string targetPath = "Text";
            var targetObj = new object();
            var builder = new BindingBuilder();
            var src = new BindingSourceModel { ObjectProperty = new BindingSourceModel { StringProperty = "test" } };
            builder.Bind(targetObj, targetPath).To<BindingSourceModel>(() => model => model.GetObjectProperty().Member<string>("StringProperty").Member<int>("Length"));

            IList<Func<IDataContext, IBindingSource>> sources = builder.GetData(BindingBuilderConstants.Sources);
            IBindingSource source = sources.Single().Invoke(builder);
            BindingParserTest.BindingSourceShouldBeValidDataContext(targetObj, source, string.Empty);
            var sourceObj = new BindingSourceModel();
            BindingServiceProvider.ContextManager.GetBindingContext(targetObj).Value = sourceObj;
            BindingParserTest.BindingSourceShouldBeValidDataContext(targetObj, source, string.Empty);

            var data = builder.GetData(BindingBuilderConstants.MultiExpression);
            data.Invoke(builder, new[] { src }).ShouldEqual(((BindingSourceModel)src.GetObjectProperty()).StringProperty.Length);
        }

        [TestMethod]
        public void BuilderShouldUseDynamicMember4()
        {
            const string targetPath = "Text";
            var targetObj = new object();
            var builder = new BindingBuilder();
            var src = new BindingSourceModel { ObjectProperty = "test" };
            builder.Bind(targetObj, targetPath).To(src, () => model => model.Member<string>("ObjectProperty").Member<int>("Length"));

            IList<Func<IDataContext, IBindingSource>> sources = builder.GetData(BindingBuilderConstants.Sources);
            IBindingSource source = sources.Single().Invoke(builder);
            source.Path.Path.ShouldEqual("ObjectProperty.Length");
            source.GetSource(true).ShouldEqual(src);
        }

        [TestMethod]
        public void BuilderShouldUseDynamicMember5()
        {
            const string targetPath = "Text";
            var targetObj = new object();
            var builder = new BindingBuilder();
            var src = new BindingSourceModel { ObjectProperty = "test" };
            builder.Bind(targetObj, targetPath).To(src, () => model => model.Member<string>("ObjectProperty").Member<int>("Length") + 0);

            IList<Func<IDataContext, IBindingSource>> sources = builder.GetData(BindingBuilderConstants.Sources);
            IBindingSource source = sources.Single().Invoke(builder);
            source.Path.Path.ShouldEqual("ObjectProperty.Length");
            source.GetSource(true).ShouldEqual(src);

            var data = builder.GetData(BindingBuilderConstants.MultiExpression);
            data.Invoke(builder, new object[] { ((string)src.ObjectProperty).Length }).ShouldEqual(((string)src.ObjectProperty).Length);
        }

        #endregion
    }
}