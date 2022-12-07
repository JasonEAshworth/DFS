using Valid_DynamicFilterSort.Base;

namespace Valid_DynamicFilterSort.DynamicLinq
{
    /// <summary>
    /// Module Registration
    /// </summary>
    public class DynamicLinq : BaseDataInterfaceRegistration
    {
        public override string InterfaceType { get; } = nameof(DynamicLinq);

        /// <summary>
        /// Use a parameter constructor in module registration to register
        /// IDataSyntaxBuilder and IDataSyntaxBuilder&lt;TParameter&gt;
        /// </summary>
        public DynamicLinq()
        {
            // base syntax builder, that farms out work to the Filter and Sort specific versions
            // in theory, one builder is all you need if you don't mind intermingling logic
            base.AddDataSyntaxBuilder<DynamicLinqDataSyntaxBuilder>();
            // filter syntax builder
            base.AddDataSyntaxBuilder<DynamicLinqFilterDataSyntaxBuilder>();
            // sort syntax builder
            base.AddDataSyntaxBuilder<DynamicLinqSortDataSyntaxBuilder>();
            // set the data accessor
            base.UseDataAccessor<DynamicLinqDataAccessor>();
        }
    }
}