using System.Collections.Generic;

namespace Schubert.Framework.Environment.Modules
{
    public delegate void FeatureDependencyNotificationHandler(string messageFormat, string featureId, IEnumerable<string> featureIds);

    public interface IFeatureManager
    {
        event FeatureDependencyNotificationHandler FeatureDependencyNotification;

        /// <summary>
        /// 获取所有启用的 feature（根据 shell 存储的 feature 信息来判断可用性）。
        /// </summary>
        /// <returns>返回可用 feature 的集合。</returns>
        IEnumerable<Feature> GetEnabledFeatures();

        /// <summary>
        /// 获取所有可用的 feature（检查项目中包含的全部 feature）。
        /// </summary>
        /// <returns>返回可用 feature 的集合。</returns>
        IEnumerable<Feature> GetAvailableFeatures();

        /// <summary>
        /// 启用指定的 feature。
        /// </summary>
        /// <param name="featureNames">要启用的 feature 名称枚举。</param>
        /// <param name="force">如果为 true 启用 feature 时会自动启用它的依赖项 feature，否则，如果依赖项未启用该 feature 也不会启用。</param>
        /// <returns>返回被启用的 feature 名称集合。</returns>
        IEnumerable<string> EnableFeatures(IEnumerable<string> featureNames, bool force = true);


        /// <summary>
        /// 禁用指定的 feature。
        /// </summary>
        /// <param name="featureNames">要禁用的 feature 名称枚举。</param>
        /// <param name="force">如果为 true 禁用 feature 时会自动禁用依赖它的其他 feature，否则，如果有其他 feature 依赖该 feature，则该 feature 不会被禁用。</param>
        /// <returns>返回被禁用的 feature 名称集合。</returns>
        IEnumerable<string> DisableFeatures(IEnumerable<string> featureNames, bool force = true);

        /// <summary>
        /// 返回依赖于指定名称的 feature 的 feature 集合。
        /// </summary>
        /// <param name="featureName">feature 名称。</param>
        /// <returns>依赖与 featureName 的 feature 名称集合。</returns>
        IEnumerable<string> GetDependentFeatures(string featureName);
    }
}