using System.ComponentModel;

namespace Firefly.Models.Responses;

public enum FriendlyHttpStatusCode
{
    /* 信息响应 (100–199) */

    [Description("这个临时响应表明，迄今为止的所有内容都是可行的，客户端应该继续请求，如果已经完成，则忽略它。")]
    Continue = 100,

    [Description("该代码是响应客户端的 Upgrade (en-US) 请求头发送的，指明服务器即将切换的协议。")]
    SwitchingProtocols = 101,

    [Description("此代码表示服务器已收到并正在处理该请求，但当前没有响应可用。")]
    Processing = 102,

    [Description("此状态代码主要用于与 Link 链接头一起使用，以允许用户代理在服务器准备响应阶段时开始预加载 preloading 资源。")]
    EarlyHints = 103,

    /* 成功响应 (200–299) */

    [Description("请求成功。")]
    OK = 200,

    [Description("该请求已成功，并因此创建了一个新的资源。这通常是在 POST 请求，或是某些 PUT 请求之后返回的响应。")]
    Created = 201,

    [Description("请求已经接收到，但还未响应，没有结果。")]
    Accepted = 202,

    [Description("服务器已成功处理了请求，但返回的实体头部元信息不是在原始服务器上有效的确定集合，而是来自本地或者第三方的拷贝。")]
    NonAuthoritativeInformation = 203,

    [Description("对于该请求没有的内容可发送，但头部字段可能有用。")]
    NoContent = 204,

    [Description("告诉用户代理重置发送此请求的文档。")]
    ResetContent = 205,

    [Description("当从客户端发送Range范围标头以只请求资源的一部分时，将使用此响应代码。")]
    PartialContent = 206,

    [Description("对于多个状态代码都可能合适的情况，传输有关多个资源的信息。")]
    MultiStatus = 207,

    [Description("在 DAV 里面使用 <dav:propstat> 响应元素以避免重复枚举多个绑定的内部成员到同一个集合。")]
    AlreadyReported = 208,

    [Description("服务器已经完成了对资源的 GET 请求，并且响应是对当前实例应用的一个或多个实例操作结果的表示。")]
    IMUsed = 226,

    /* 重定向消息 (300–399) */

    [Description("请求拥有多个可能的响应。用户代理或者用户应当从中选择一个。")]
    MultipleChoices = 300,

    [Description("请求资源的 URL 已永久更改。在响应中给出了新的 URL。")]
    MovedPermanently = 301,

    [Description("此响应代码表示所请求资源的 URI 已暂时更改。未来可能会对 URI 进行进一步的改变。因此，客户机应该在将来的请求中使用这个相同的 URI。")]
    Found = 302,

    [Description("服务器发送此响应，以指示客户端通过一个 GET 请求在另一个 URI 中获取所请求的资源。")]
    SeeOther = 303,

    [Description("这是用于缓存的目的。它告诉客户端响应还没有被修改，因此客户端可以继续使用相同的缓存版本的响应。")]
    NotModified = 304,

    [Description("在 HTTP 规范中定义，以指示请求的响应必须被代理访问。由于对代理的带内配置的安全考虑，它已被弃用。")]
    UseProxy = 305,

    [Description("此响应代码不再使用；它只是保留。它曾在 HTTP/1.1 规范的早期版本中使用过。")]
    Unused = 306,

    [Description("服务器发送此响应，以指示客户端使用在前一个请求中使用的相同方法在另一个 URI 上获取所请求的资源。")]
    TemporaryRedirect = 307,

    [Description("这意味着资源现在永久位于由 Location: HTTP Response 标头指定的另一个 URI。")]
    PermanentRedirect = 308,

    /* 客户端错误响应 (400–499) */

    [Description("由于被认为是客户端错误（例如，错误的请求语法、无效的请求消息帧或欺骗性的请求路由），服务器无法或不会处理请求。")]
    BadRequest = 400,

    [Description("客户端必须对自身进行身份验证才能获得请求的响应。")]
    Unauthorized = 401,

    [Description("此响应代码保留供将来使用。创建此代码的最初目的是将其用于数字支付系统，但是此状态代码很少使用，并且不存在标准约定。")]
    PaymentRequired = 402,

    [Description("客户端没有访问内容的权限；也就是说，它是未经授权的，因此服务器拒绝提供请求的资源。")]
    Forbidden = 403,

    [Description("服务器找不到请求的资源。")]
    NotFound = 404,

    [Description("服务器知道请求方法，但目标资源不支持该方法。")]
    MethodNotAllowed = 405,

    [Description("当 web 服务器在执行服务端驱动型内容协商机制后，没有发现任何符合用户代理给定标准的内容时，就会发送此响应。")]
    NotAcceptable = 406,

    [Description("类似于 401 Unauthorized 但是认证需要由代理完成。")]
    ProxyAuthenticationRequired = 407,

    [Description("此响应由一些服务器在空闲连接上发送，即使客户端之前没有任何请求。这意味着服务器想关闭这个未使用的连接。")]
    RequestTimeout = 408,

    [Description("当请求与服务器的当前状态冲突时，将发送此响应。")]
    Conflict = 409,

    [Description("当请求的内容已从服务器中永久删除且没有转发地址时，将发送此响应。客户端需要删除缓存和指向资源的链接。")]
    Gone = 410,

    [Description("服务端拒绝该请求因为 Content-Length 头部字段未定义但是服务端需要它。")]
    LengthRequired = 411,

    [Description("客户端在其头文件中指出了服务器不满足的先决条件。")]
    PreconditionFailed = 412,

    [Description("请求实体大于服务器定义的限制。服务器可能会关闭连接，或在标头字段后返回重试 Retry-After。")]
    RequestEntityTooLarge = 413,

    [Description("客户端请求的 URI 比服务器愿意接收的长度长。")]
    RequestUriTooLong = 414,

    [Description("服务器不支持请求数据的媒体格式，因此服务器拒绝请求。")]
    UnsupportedMediaType = 415,

    [Description("无法满足请求中 Range 标头字段指定的范围。该范围可能超出了目标 URI 数据的大小。")]
    RequestedRangeNotSatisfiable = 416,

    [Description("此响应代码表示服务器无法满足 Expect 请求标头字段所指示的期望。")]
    ExpectationFailed = 417,

    [Description("服务端拒绝用茶壶煮咖啡。笑话，典故来源茶壶冲泡咖啡。")]
    Im_a_teapot = 418,

    [Description("请求被定向到无法生成响应的服务器。这可以由未配置为针对请求 URI 中包含的方案和权限组合生成响应的服务器发送。")]
    MisdirectedRequest = 421,

    [Description("请求格式正确，但由于语义错误而无法遵循。")]
    UnprocessableEntity = 422,

    [Description("正在访问的资源已锁定。")]
    Locked = 423,

    [Description("由于前一个请求失败，请求失败。")]
    FailedDependency = 424,

    [Description("表示服务器不愿意冒险处理可能被重播的请求。")]
    TooEarly = 425,

    [Description("服务器拒绝使用当前协议执行请求，但在客户端升级到其他协议后可能愿意这样做。")]
    UpgradeRequired = 426,

    [Description("源服务器要求请求是有条件的。")]
    PreconditionRequired = 428,

    [Description("用户在给定的时间内发送了太多请求（\"限制请求速率\"）。")]
    TooManyRequests = 429,

    [Description("服务器不愿意处理请求，因为其头字段太大。")]
    RequestHeaderFieldsTooLarge = 431,

    [Description("用户代理请求了无法合法提供的资源，例如政府审查的网页。")]
    UnavailableForLegalReasons = 451,

    /* 服务端错误响应 (500–599) */

    [Description("服务器遇到了不知道如何处理的情况。")]
    InternalServerError = 500,

    [Description("服务器不支持请求方法，因此无法处理。")]
    NotImplemented = 501,

    [Description("此错误响应表明服务器作为网关需要得到一个处理这个请求的响应，但是得到一个错误的响应。")]
    BadGateway = 502,

    [Description("服务器没有准备好处理请求。常见原因是服务器因维护或重载而停机。")]
    ServiceUnavailable = 503,

    [Description("当服务器充当网关且无法及时获得响应时，会给出此错误响应。")]
    GatewayTimeout = 504,

    [Description("服务器不支持请求中使用的 HTTP 版本。")]
    HttpVersionNotSupported = 505,

    [Description("服务器存在内部配置错误：所选的变体资源被配置为参与透明内容协商本身，因此不是协商过程中的适当终点。")]
    VariantAlsoNegotiates = 506,

    [Description("无法在资源上执行该方法，因为服务器无法存储成功完成请求所需的表示。")]
    InsufficientStorage = 507,

    [Description("服务器在处理请求时检测到无限循环。")]
    LoopDetected = 508,

    [Description("服务器需要对请求进行进一步扩展才能完成请求。")]
    NotExtended = 510,

    [Description("指示客户端需要进行身份验证才能获得网络访问权限。")]
    NetworkAuthenticationRequired = 511
}
