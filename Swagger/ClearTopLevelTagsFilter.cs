using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace His.Api.Swagger;

/// <summary>
/// 清空 OpenAPI 文档顶层的 tags 定义。
/// <para>Swashbuckle 会按控制器名（Auth、Billing...）预生成顶层 tag，
/// Swagger UI 会把这些没有接口引用的 tag 渲染成空分组。</para>
/// <para>本项目分组完全由各接口的 [SwaggerOperation(Tags = ...)] 决定，
/// 因此顶层 tag 定义是多余的，直接清空。</para>
/// </summary>
public class ClearTopLevelTagsFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Tags?.Clear();
    }
}
