#include "pch-cpp.hpp"

#ifndef _MSC_VER
# include <alloca.h>
#else
# include <malloc.h>
#endif


#include <limits>


struct VirtualActionInvoker0
{
	typedef void (*Action)(void*, const RuntimeMethod*);

	static inline void Invoke (Il2CppMethodSlot slot, RuntimeObject* obj)
	{
		const VirtualInvokeData& invokeData = il2cpp_codegen_get_virtual_invoke_data(slot, obj);
		((Action)invokeData.methodPtr)(obj, invokeData.method);
	}
};
template <typename T1, typename T2, typename T3>
struct VirtualActionInvoker3
{
	typedef void (*Action)(void*, T1, T2, T3, const RuntimeMethod*);

	static inline void Invoke (Il2CppMethodSlot slot, RuntimeObject* obj, T1 p1, T2 p2, T3 p3)
	{
		const VirtualInvokeData& invokeData = il2cpp_codegen_get_virtual_invoke_data(slot, obj);
		((Action)invokeData.methodPtr)(obj, p1, p2, p3, invokeData.method);
	}
};
template <typename R>
struct VirtualFuncInvoker0
{
	typedef R (*Func)(void*, const RuntimeMethod*);

	static inline R Invoke (Il2CppMethodSlot slot, RuntimeObject* obj)
	{
		const VirtualInvokeData& invokeData = il2cpp_codegen_get_virtual_invoke_data(slot, obj);
		return ((Func)invokeData.methodPtr)(obj, invokeData.method);
	}
};
template <typename R, typename T1, typename T2, typename T3>
struct VirtualFuncInvoker3
{
	typedef R (*Func)(void*, T1, T2, T3, const RuntimeMethod*);

	static inline R Invoke (Il2CppMethodSlot slot, RuntimeObject* obj, T1 p1, T2 p2, T3 p3)
	{
		const VirtualInvokeData& invokeData = il2cpp_codegen_get_virtual_invoke_data(slot, obj);
		return ((Func)invokeData.methodPtr)(obj, p1, p2, p3, invokeData.method);
	}
};
template <typename R, typename T1, typename T2, typename T3, typename T4>
struct VirtualFuncInvoker4
{
	typedef R (*Func)(void*, T1, T2, T3, T4, const RuntimeMethod*);

	static inline R Invoke (Il2CppMethodSlot slot, RuntimeObject* obj, T1 p1, T2 p2, T3 p3, T4 p4)
	{
		const VirtualInvokeData& invokeData = il2cpp_codegen_get_virtual_invoke_data(slot, obj);
		return ((Func)invokeData.methodPtr)(obj, p1, p2, p3, p4, invokeData.method);
	}
};
template <typename R, typename T1, typename T2, typename T3>
struct GenericVirtualFuncInvoker3
{
	typedef R (*Func)(void*, T1, T2, T3, const RuntimeMethod*);

	static inline R Invoke (const RuntimeMethod* method, RuntimeObject* obj, T1 p1, T2 p2, T3 p3)
	{
		VirtualInvokeData invokeData;
		il2cpp_codegen_get_generic_virtual_invoke_data(method, obj, &invokeData);
		return ((Func)invokeData.methodPtr)(obj, p1, p2, p3, invokeData.method);
	}
};
template <typename R, typename T1, typename T2, typename T3>
struct InterfaceFuncInvoker3
{
	typedef R (*Func)(void*, T1, T2, T3, const RuntimeMethod*);

	static inline R Invoke (Il2CppMethodSlot slot, RuntimeClass* declaringInterface, RuntimeObject* obj, T1 p1, T2 p2, T3 p3)
	{
		const VirtualInvokeData& invokeData = il2cpp_codegen_get_interface_invoke_data(slot, obj, declaringInterface);
		return ((Func)invokeData.methodPtr)(obj, p1, p2, p3, invokeData.method);
	}
};
template <typename R, typename T1, typename T2, typename T3>
struct GenericInterfaceFuncInvoker3
{
	typedef R (*Func)(void*, T1, T2, T3, const RuntimeMethod*);

	static inline R Invoke (const RuntimeMethod* method, RuntimeObject* obj, T1 p1, T2 p2, T3 p3)
	{
		VirtualInvokeData invokeData;
		il2cpp_codegen_get_generic_interface_invoke_data(method, obj, &invokeData);
		return ((Func)invokeData.methodPtr)(obj, p1, p2, p3, invokeData.method);
	}
};

struct Action_1_t6F9EB113EB3F16226AEF811A2744F4111C116C87;
struct Dictionary_2_t403063CE4960B4F46C688912237C6A27E550FF55;
struct Dictionary_2_t5C8F46F5D57502270DD9E1DA8303B23C7FE85588;
struct Func_1_tD59A12717D79BFB403BF973694B1BE5B85474BD1;
struct Predicate_1_t8342C85FF4E41CD1F7024AC0CDC3E5312A32CB12;
struct Predicate_1_t7F48518B008C1472339EEEBABA3DE203FE1F26ED;
struct TaskFactory_1_t0BEF06D58E44525B9135AB0B22D016856EE69FF3;
struct Task_1_t4C228DE57804012969575431CFF12D57C875552D;
struct Task_1_t0C4CD3A5BB93A184420D73218644C56C70FDA7E2;
struct Task_1_t3D7638C82ED289AF156EDBAE76842D8DF4C4A9E0;
struct Task_1_t7A5D3E6F94872974689076E6A3A64E3104677A40;
struct ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031;
struct CharU5BU5D_t799905CF001DD5F13F7DBB310181FC4D8B7D0AAB;
struct CipherSuiteCodeU5BU5D_t61EC0E6F53394985FFC36DEB587C70F4EE26D435;
struct DelegateU5BU5D_tC5AB7E8F745616680F337909D3A8E6C722CDF771;
struct Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C;
struct IntPtrU5BU5D_tFD177F8C806A6921AD7150264CCC62FA00CAD832;
struct ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918;
struct StackTraceU5BU5D_t32FBCB20930EAF5BAE3F450FF75228E5450DA0DF;
struct StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248;
struct TypeU5BU5D_t97234E1129B564EB38B8D85CAC2AD8B5B9522FFB;
struct WebSocketStateU5BU5D_t3998D3C6855D91A27627D1D6135ADC884B2E4C1B;
struct Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07;
struct ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263;
struct ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129;
struct ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F;
struct ArrayList_t7A8E5AF0C4378015B5731ABE2BED8F2782FEEF8A;
struct AsyncCallback_t7FEF460CBDCFB9C5FA2EF776984778B9A4145F4C;
struct AsyncProtocolRequest_tF31CF30F8C57A3D5DC10A9FB0A5568FA254DFE86;
struct AuthenticatedStream_t8DCF41E151F705E2494FC7836F5E2EF7C539FA39;
struct BufferOffsetSize2_t2C3F7F42F64D84F357DC24AFCD44AC4679B20629;
struct BypassElementCollection_t15465BC07F8DC8BABA6953182904AF762D638DBE;
struct CancellationTokenSource_tAAE1E0033BCFC233801F8CB4CED5C852B350CB7B;
struct ConfigurationElement_tAE3EE71C256825472831FFBB7F491275DFAF089E;
struct ConfigurationPropertyCollection_t1DEB95D3283BB11A46B862E9D13710ED698B6C93;
struct ConnectionManagementElementCollection_tEEB0BEE8289364CCA1D76DDA3F862429612DE0F1;
struct ConnectionManagementSection_t8F4D45BE147F24F54F2124CC800B4594031F41C2;
struct ContextCallback_tE8AFBDBFCC040FDA8DA8C1EEFE9BD66B16BDA007;
struct DefaultProxySection_tC0247A534955DC9D887B03AA4EFB1C1E01AD1E34;
struct Delegate_t;
struct DelegateData_t9B286B493293CD2D23A5B2B5EF0E5B1324C2B77E;
struct DiagnosticsConfigurationHandler_t0C4B611E2C6EA17B288C43070FAF65F96DCA96E7;
struct Encoding_t65CDEF28CF20A7B8C92E85A4E808920C2465F095;
struct Exception_t;
struct ExceptionDispatchInfo_tD7AF19E75FEC22F4A8329FD1E9EDF96615CB2757;
struct HttpWebRequestElement_t8D2FEEC531D1FBE1B6E3D1077350A1EBDCB77DC3;
struct IAsyncResult_t7B9B5A0ECB35DCEC31B8A8122C37D687369253B5;
struct IAsyncStateMachine_t0680C7F905C553076B552D5A1A6E39E2F0F36AA2;
struct ICertificateValidator_t8ECF48A37EC708C8A16305F9B6B44C50676AA854;
struct IDictionary_t6D03155AF1FA9083817AA5B6AD7DEEACC26AB220;
struct IFormatterConverter_t726606DAC82C384B08C82471313C340968DDB609;
struct Ipv6Element_t480208D0E23E612DC1FD1CE19D9FB491F0F66932;
struct LocalCertSelectionCallback_t3AB79A8AA642074A3EF886118963EEA3CCABABE2;
struct ManagedWebSocket_tFE006AE2E3A7EFDD703AAA7AC6FC035F65695F58;
struct MethodInfo_t;
struct MobileAuthenticatedStream_tD0306DC2B0CDA3C7DB261C19FFA35CA8EE24309E;
struct MobileTlsContext_t456DDC89866EE34EF32EEF959AD92C9F17684476;
struct MobileTlsProvider_tD60D82BEBF267F50F388A026DBB092C7188BB017;
struct MonoLocalCertificateSelectionCallback_t34F7772BA5ECE38E6CBD4C311F579DD1D4724DE3;
struct MonoRemoteCertificateValidationCallback_t1A389B61998873F6B9A2EE7A11C36333A8AECCA0;
struct MonoTlsProvider_t39C898CDC9458EEAD7C019B4B23701EAF9E24F7E;
struct MonoTlsSettings_tD79AF4AE5C2CD533A3D7A08FED479B1EC1A031B0;
struct NetSectionGroup_tA83DCAF89773087D0E921B92F67441132B71D52F;
struct NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A;
struct ObjectDisposedException_tC5FB29E8E980E2010A2F6A5B9B791089419F89EB;
struct PerformanceCountersElement_tCB62C8CA7C218E35653DC5BB690A5FFD6D608B30;
struct PlatformNotSupportedException_tD2BD7EB9278518AA5FE8AE75AD5D0D4298A4631A;
struct ProxyElement_t4D277820F9C77333BE8CC6498DE7E06C36868C65;
struct RandomNumberGenerator_t4E862666A2F7D55324960670C7A1B4C2D40222F3;
struct RemoteCertificateValidationCallback_t2F4C5801F96B2C2BF934511796C5BFEAEBF01955;
struct SafeSerializationManager_tCBB85B95DFD1634237140CD892E82D06ECB3F5E6;
struct SemaphoreSlim_t0D5CB5685D9BFA5BF95CEC6E7395490F933E8DB2;
struct SerializationInfo_t3C47F63E24BEB9FCE2DC6309E027F238DC5C5E37;
struct ServerCertSelectionCallback_t653386CAEAE0236FCF61A92963AB1646BB23C654;
struct ServicePointManagerElement_t98ED8374270537E77E57D014ECEE8B1A3A8EEFF2;
struct SettingsSection_tC931BA69B22D5E0ED07698F02778482987319A12;
struct SocketElement_tDC83FCE3C280D8D590972F989292C30E2B7F6CC6;
struct SslClientAuthenticationOptions_tA5277DAACAFB33887D2513D9399C5154B0D5CBC9;
struct SslStream_t19A079881850F9CAD7BAA6FB625BBC4647ED5A27;
struct StackGuard_tACE063A1B7374BDF4AD472DE4585D05AD8745352;
struct Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE;
struct String_t;
struct StringBuilder_t;
struct Task_t751C4CC3ECD055BABA8A0B6A5DFBB4283DCA8572;
struct TaskFactory_tF781BD37BE23917412AD83424D1497C7C1509DF0;
struct TaskScheduler_t3F0550EBEF7C41F74EC8C08FF4BED0D8CE66006E;
struct Timer_t763C1D5F5A36087DC92C7DA4D1F8AB578F83AB00;
struct Type_t;
struct UTF8Encoding_t90B56215A1B0B7ED5CDEA772E695F0DDAFBCD3BE;
struct Void_t4861ACF8F4594C3437BB48B6E56783494B843915;
struct WebProxyScriptElement_t0C0E2338F7F42D5D386F5AE68FF7F870FA6E7167;
struct WebRequestModuleElementCollection_t5979A19A48B822831C1D351EB2AA25058C7F7463;
struct WebRequestModulesSection_t4099F9890D7851314C5528C577E4C51B36C97D00;
struct WebSocket_tE3DEAFF0F68BD5AF9A526D29D56B6E5C6E24C3A1;
struct WebSocketException_t7ED58EA50E372ED89E1469A44379C2FB097A6263;
struct WebSocketReceiveResult_t31FCE9F2E53843C065CD57BE10581EE47D6FAD2C;
struct Win32Exception_t15A75629914EB77C816D8219D93ED78E50C74BE9;
struct X509Certificate_t966CC553AF25AE7991F5B4C2AACBCF6C66C8F9C4;
struct X509CertificateCollection_t2900D71D188EDCA7DEB5077D36103EE5DA6805CE;
struct X509CertificateImpl_tF590E81705CE1FE152C590E5A875D4FE3BE348EF;
struct X509Chain_t2E7F88A4C97CAB82AF938F13A11354A0475469E5;
struct X509ChainImpl_tBA1BF154DB9DC321EE068BBC53C7CF43CAA2621F;
struct XmlNode_t3180B9B3D5C36CD58F5327D9F13458E3B3F030AF;
struct Utf8MessageState_tAC717AAC548FCA700FE47E9DF4FFCC61E55AF91D;
struct ReadWriteTask_t0821BF49EE38596C7734E86E1A6A39D769BE2C05;
struct ContingentProperties_t3FA59480914505CEA917B1002EC675F29D0CB540;

IL2CPP_EXTERN_C RuntimeClass* ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* AsyncTaskMethodBuilder_1_tA28B0962049DC249BEC0054E2394B1962E28E90E_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* Exception_t_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* ManagedWebSocket_tFE006AE2E3A7EFDD703AAA7AC6FC035F65695F58_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* Marshal_tD976A56A90263C3CE2B780D4B1CADADE2E70B4A7_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* MobileTlsProvider_tD60D82BEBF267F50F388A026DBB092C7188BB017_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* ObjectDisposedException_tC5FB29E8E980E2010A2F6A5B9B791089419F89EB_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* PlatformNotSupportedException_tD2BD7EB9278518AA5FE8AE75AD5D0D4298A4631A_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* StringBuilder_t_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* TimeSpan_t8195C5B013A2C532FEBDF0B64B6911982E750F5A_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* Timeout_tDBB45B56C59D1AF9A6B1EC5F1E3200D12AB18E41_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* WebSocketError_tEC1353C241C6481FE77428ABEB57E39B8E6E7672_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* WebSocketHandle_t9C8D33BD3D8553B1FD363A68ABDF5B81FC089F50_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C String_t* _stringLiteral0095EE92A43C893ED16C93F6273DD887E60677F6;
IL2CPP_EXTERN_C String_t* _stringLiteral07624473F417C06C74D59C64840A1532FCE2C626;
IL2CPP_EXTERN_C String_t* _stringLiteral08F6264E77829B6ED2AD7EB9B887F08ED5AEEEE8;
IL2CPP_EXTERN_C String_t* _stringLiteral14AEEE32A8599AABC74AA8FC4206A0BF642F3F20;
IL2CPP_EXTERN_C String_t* _stringLiteral1F40F39B9F659411903AC93DF93E8658291E5522;
IL2CPP_EXTERN_C String_t* _stringLiteral215B9008FEB54933AF219588889451A0CB610D34;
IL2CPP_EXTERN_C String_t* _stringLiteral3CE4531C9B5C100D000F5224788AD0FC73FBFBB9;
IL2CPP_EXTERN_C String_t* _stringLiteral46F273EF641E07D271D91E0DC24A4392582671F8;
IL2CPP_EXTERN_C String_t* _stringLiteral491C803A42402C98C26F444D4AFAEB35E12580D4;
IL2CPP_EXTERN_C String_t* _stringLiteral50C6B78B527F35406BA05478189F5D0C7CC514B4;
IL2CPP_EXTERN_C String_t* _stringLiteral5246246403494F3045678A959CC58FB0501C504D;
IL2CPP_EXTERN_C String_t* _stringLiteral6FE6AA6EF03FB6550152903A7B201A6108F728DB;
IL2CPP_EXTERN_C String_t* _stringLiteral73E7438EC9EFF139A2E1CEA81F03B87F9EF5CF0A;
IL2CPP_EXTERN_C String_t* _stringLiteral8A09CD330FFB1377C86A2013D5C1E2F907AE1682;
IL2CPP_EXTERN_C String_t* _stringLiteral8CAC69067702430A6F00B8758861B928C4AA88C4;
IL2CPP_EXTERN_C String_t* _stringLiteral8CE29B903D391961B0EC3D0BC630ABAF3D763A92;
IL2CPP_EXTERN_C String_t* _stringLiteral95C3DD8E6E01DC5FE6AC3E2B6375E6AD246F8241;
IL2CPP_EXTERN_C String_t* _stringLiteral969FB05E796DDD94836D21AF137D831A5BAE54DC;
IL2CPP_EXTERN_C String_t* _stringLiteral9A1C57A14DBAA067A98C8B33FA6B28D793AFE67D;
IL2CPP_EXTERN_C String_t* _stringLiteralA6EAFAF4139980ACB770E237272125EB9180B7A6;
IL2CPP_EXTERN_C String_t* _stringLiteralA8327EDA8A27CFF2C0328F41898D104AE50B8B3B;
IL2CPP_EXTERN_C String_t* _stringLiteralAA0F00452EE7B525EB6EC13E514BABDE9C00576A;
IL2CPP_EXTERN_C String_t* _stringLiteralBDC5FDC47ACE7F03CD95EC2B3BB2812C5F79E14C;
IL2CPP_EXTERN_C String_t* _stringLiteralCB9340620E0F239BC39D9B1AA9CCF6119A0BB3FF;
IL2CPP_EXTERN_C String_t* _stringLiteralDA5FA4E2B91DB334E0C213C29BD90197D7DD1670;
IL2CPP_EXTERN_C String_t* _stringLiteralE1BC7174A658D0FC8737866D17DA79C9ED801F4E;
IL2CPP_EXTERN_C String_t* _stringLiteralE7640D55535D38F6521AEF64C237E62A7B71E76D;
IL2CPP_EXTERN_C const RuntimeMethod* AsyncTaskMethodBuilder_1_AwaitUnsafeOnCompleted_TisConfiguredTaskAwaiter_t8FC486401D1951F2426478F2050EE290197B6E80_TisU3CReadResponseHeaderLineAsyncU3Ed__32_tEEAA4B9F7D1F99D0F6B270421640B301B419757B_m43E8610E0472D376CD75485C575F5C9C8D91AE36_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* AsyncTaskMethodBuilder_1_SetException_m897218C2DB8D7BA446EB263573085805B8F0638C_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* AsyncTaskMethodBuilder_1_SetResult_mB79121C0ACAC704500FAA7350ED0FE6604DDA05E_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* AsyncTaskMethodBuilder_1_SetStateMachine_mE4D48B78E50CA2A3650A856ACF86A3CFBA249C30_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* AuthenticatedStream__ctor_m8B163146A01B938A3E87E1BED07EE07494A3E357_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* BypassElementCollection__ctor_m344EC8CD2EACA36E1213C5914037BB133E87FE54_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* ConfiguredTaskAwaitable_1_GetAwaiter_m7A77B1981FEC19CC7E1570EDC3F16AC5C14B4439_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* ConfiguredTaskAwaiter_GetResult_mF1FF6CBD66A3F581D413793BA8C2AF58B707D9CC_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* ConfiguredTaskAwaiter_get_IsCompleted_mD70263ED42C8E379EE20DBC2F218C3E629B4B4D3_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* ConnectionManagementElementCollection__ctor_m4698342A715032A762FA04B2745BFCA46263BCB3_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* ConnectionManagementSection__ctor_m2FA1D29F887BD6498FBEF692BBC0AA727E38E802_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* ConnectionManagementSection_get_Properties_mFCE0F9418E9E8CD2E1301815CC4D548261F58E95_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* DefaultProxySection_Reset_m0B9A1155840E7C55FC24AEDB23FB17A2815EEB77_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* DefaultProxySection__ctor_m962B68CEFCDE7DFB7646EF16297CA4BE0097284D_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* DefaultProxySection_get_Properties_m5186F1A2734EB96F065C7BC14A215D17F8325418_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* DiagnosticsConfigurationHandler_Create_mF21B1CC68B87DE078690B2B493FCDC5F728CBAAB_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* DiagnosticsConfigurationHandler__ctor_mDFB0000E6218840F66D21BB6C9BF3F647CCBC5A3_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* HttpWebRequestElement__ctor_mB9DDAC7FF431D4AF4CAFCBC96E1A3572A1F83717_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* HttpWebRequestElement_get_Properties_mAA0F11EA73B5397C50809EAC679F2C564F9EC7CA_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* Ipv6Element__ctor_m869AC261E0994A8B3D83CEE25C6CB99E6A5EA4C1_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* Ipv6Element_get_Properties_m382D5B5B098EFF19C2A4AC1889CD1C8EAB918322_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* NetSectionGroup__ctor_mCD2F0DC2B9F0C6C346BC91BD8750C7111A9528AC_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* PerformanceCountersElement__ctor_m9537195CBC7DC607F7253D06E7B6B1090456E68F_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* PerformanceCountersElement_get_Properties_m7C16FBC3E915E8384FB4409209628737B1537C9B_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* ProxyElement__ctor_mC1BDF426F97F4F84CB5A34B423F59EC8748EBD70_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* ProxyElement_get_Properties_mBFE6E283FE7E9329B586ACAADC8F2E047C7ABB2A_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* ServicePointManagerElement__ctor_m0F1CF21589CA283DA734A472930A9665698B2D36_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* ServicePointManagerElement_get_Properties_m70469523AAD1A2C2E9D4F90DDA1BA619FCE68B58_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* SettingsSection__ctor_mBD94E7E098221DC09775A3AE1713930F6820D242_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* SettingsSection_get_Properties_mA774C9C884F4AD5DEC2BBE31DF7E35CAC71EBF8F_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* SocketElement__ctor_m4FB4C851F1BB20CED74C206EFB1BA9DDFBE024D2_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* SocketElement_get_Properties_m8AF94DEEDA0EB4092F59FBDC361592E1B9EBDEC3_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* SslClientAuthenticationOptions_set_CertificateRevocationCheckMode_m5DB15346690DAC5A8F610AA8C7DEC7F2DFC29DE9_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* SslClientAuthenticationOptions_set_EncryptionPolicy_m008E6F28D3C4BFEFFB32A9BEA6AEF1C2B53F1C77_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* SslStream_CheckDisposed_m884E18F119B366EE6734CCFA126A1F633687FA95_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* SslStream_Seek_mFEDBF392998DDDB0F7A71F25D1BB821EA1463B25_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* SslStream_set_Position_m70987A87C7DB346D2FD447EA11E96774105EE5D2_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* TaskToApm_End_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mCA309E122256972A27C74FC814F47AD13AF623ED_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* Task_1_ConfigureAwait_m8203F4D13209C12845066A383E5B850D0486B209_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* ThrowStub_ThrowNotSupportedException_m14168B43936EF78B7B8AC08BD815254DAA1D9FA2_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* WebProxyScriptElement__ctor_mB613D71DE569317E40726CD8DB9A6ABA0A302821_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* WebProxyScriptElement_get_Properties_mF5188225C0C1D64BC136465BB4B0E962FFE9240B_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* WebRequestModuleElementCollection__ctor_m62C156D78336B2E37EA6F10D477FDEAE4161F99E_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* WebRequestModulesSection__ctor_mF25CD80A5111A4F87B7FBF7663A3442470B97C0E_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* WebRequestModulesSection_get_Properties_mBDBDED569E677717FB617CD36B94AE34BF4208F6_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* WebSocketReceiveResult__ctor_m3B47B8C4F0CC1C96FF4B314BAAB9E67E1967E186_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* WebSocket_CreateClientWebSocket_m5E6C94310C5ED85B1A7B772B831C832EFF76FFA7_RuntimeMethod_var;
struct Delegate_t_marshaled_com;
struct Delegate_t_marshaled_pinvoke;
struct Exception_t_marshaled_com;
struct Exception_t_marshaled_pinvoke;

struct ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031;
struct DelegateU5BU5D_tC5AB7E8F745616680F337909D3A8E6C722CDF771;
struct StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248;

IL2CPP_EXTERN_C_BEGIN
IL2CPP_EXTERN_C_END

#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
struct U3CPrivateImplementationDetailsU3E_t56F624E1051A2E261613B6A81CA2333397F49CB3  : public RuntimeObject
{
};
struct CollectionBase_t44F966CC555C87F2815D668FB4586526E1C2383F  : public RuntimeObject
{
	ArrayList_t7A8E5AF0C4378015B5731ABE2BED8F2782FEEF8A* ____list;
};
struct ConfigurationElement_tAE3EE71C256825472831FFBB7F491275DFAF089E  : public RuntimeObject
{
};
struct ConfigurationPropertyCollection_t1DEB95D3283BB11A46B862E9D13710ED698B6C93  : public RuntimeObject
{
};
struct ConfigurationSectionGroup_tE7948C2D31B193F4BA8828947ED3094B952C7863  : public RuntimeObject
{
};
struct DiagnosticsConfigurationHandler_t0C4B611E2C6EA17B288C43070FAF65F96DCA96E7  : public RuntimeObject
{
};
struct MarshalByRefObject_t8C2F4C5854177FD60439EB1FCCFC1B3CFAFE8DCE  : public RuntimeObject
{
	RuntimeObject* ____identity;
};
struct MarshalByRefObject_t8C2F4C5854177FD60439EB1FCCFC1B3CFAFE8DCE_marshaled_pinvoke
{
	Il2CppIUnknown* ____identity;
};
struct MarshalByRefObject_t8C2F4C5854177FD60439EB1FCCFC1B3CFAFE8DCE_marshaled_com
{
	Il2CppIUnknown* ____identity;
};
struct MonoTlsProvider_t39C898CDC9458EEAD7C019B4B23701EAF9E24F7E  : public RuntimeObject
{
};
struct SerializationInfo_t3C47F63E24BEB9FCE2DC6309E027F238DC5C5E37  : public RuntimeObject
{
	StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* ___m_members;
	ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___m_data;
	TypeU5BU5D_t97234E1129B564EB38B8D85CAC2AD8B5B9522FFB* ___m_types;
	Dictionary_2_t5C8F46F5D57502270DD9E1DA8303B23C7FE85588* ___m_nameToIndex;
	int32_t ___m_currMember;
	RuntimeObject* ___m_converter;
	String_t* ___m_fullTypeName;
	String_t* ___m_assemName;
	Type_t* ___objectType;
	bool ___isFullTypeNameSetExplicit;
	bool ___isAssemblyNameSetExplicit;
	bool ___requireSameTokenInPartialTrust;
};
struct String_t  : public RuntimeObject
{
	int32_t ____stringLength;
	Il2CppChar ____firstChar;
};
struct StringBuilder_t  : public RuntimeObject
{
	CharU5BU5D_t799905CF001DD5F13F7DBB310181FC4D8B7D0AAB* ___m_ChunkChars;
	StringBuilder_t* ___m_ChunkPrevious;
	int32_t ___m_ChunkLength;
	int32_t ___m_ChunkOffset;
	int32_t ___m_MaxCapacity;
};
struct Task_t751C4CC3ECD055BABA8A0B6A5DFBB4283DCA8572  : public RuntimeObject
{
	int32_t ___m_taskId;
	Delegate_t* ___m_action;
	RuntimeObject* ___m_stateObject;
	TaskScheduler_t3F0550EBEF7C41F74EC8C08FF4BED0D8CE66006E* ___m_taskScheduler;
	Task_t751C4CC3ECD055BABA8A0B6A5DFBB4283DCA8572* ___m_parent;
	int32_t ___m_stateFlags;
	RuntimeObject* ___m_continuationObject;
	ContingentProperties_t3FA59480914505CEA917B1002EC675F29D0CB540* ___m_contingentProperties;
};
struct Timeout_tDBB45B56C59D1AF9A6B1EC5F1E3200D12AB18E41  : public RuntimeObject
{
};
struct ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F  : public RuntimeObject
{
};
struct ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F_marshaled_pinvoke
{
};
struct ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F_marshaled_com
{
};
struct WebSocket_tE3DEAFF0F68BD5AF9A526D29D56B6E5C6E24C3A1  : public RuntimeObject
{
};
struct X509Chain_t2E7F88A4C97CAB82AF938F13A11354A0475469E5  : public RuntimeObject
{
	X509ChainImpl_tBA1BF154DB9DC321EE068BBC53C7CF43CAA2621F* ___impl;
};
struct XmlNode_t3180B9B3D5C36CD58F5327D9F13458E3B3F030AF  : public RuntimeObject
{
};
struct ArraySegment_1_t3DC888623B720A071D69279F1FCB95A109195093 
{
	ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* ____array;
	int32_t ____offset;
	int32_t ____count;
};
struct ConfiguredTaskAwaiter_t8FC486401D1951F2426478F2050EE290197B6E80 
{
	Task_1_t4C228DE57804012969575431CFF12D57C875552D* ___m_task;
	bool ___m_continueOnCapturedContext;
};
struct Memory_1_tB7CEF4416F5014E364267478CEF016A4AC5C0036 
{
	RuntimeObject* ____object;
	int32_t ____index;
	int32_t ____length;
};
#ifndef Memory_1_t56F63672B8E752B13E0BBBBD034BA3C1F6CFDC17_marshaled_pinvoke_define
#define Memory_1_t56F63672B8E752B13E0BBBBD034BA3C1F6CFDC17_marshaled_pinvoke_define
struct Memory_1_t56F63672B8E752B13E0BBBBD034BA3C1F6CFDC17_marshaled_pinvoke
{
	Il2CppIUnknown* ____object;
	int32_t ____index;
	int32_t ____length;
};
#endif
#ifndef Memory_1_t56F63672B8E752B13E0BBBBD034BA3C1F6CFDC17_marshaled_com_define
#define Memory_1_t56F63672B8E752B13E0BBBBD034BA3C1F6CFDC17_marshaled_com_define
struct Memory_1_t56F63672B8E752B13E0BBBBD034BA3C1F6CFDC17_marshaled_com
{
	Il2CppIUnknown* ____object;
	int32_t ____index;
	int32_t ____length;
};
#endif
struct Nullable_1_t78F453FADB4A9F50F267A4E349019C34410D1A01 
{
	bool ___hasValue;
	bool ___value;
};
struct Task_1_t4C228DE57804012969575431CFF12D57C875552D  : public Task_t751C4CC3ECD055BABA8A0B6A5DFBB4283DCA8572
{
	int32_t ___m_result;
};
struct AsyncMethodBuilderCore_tD5ABB3A2536319A3345B32A5481E37E23DD8CEDF 
{
	RuntimeObject* ___m_stateMachine;
	Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* ___m_defaultContextAction;
};
struct AsyncMethodBuilderCore_tD5ABB3A2536319A3345B32A5481E37E23DD8CEDF_marshaled_pinvoke
{
	RuntimeObject* ___m_stateMachine;
	Il2CppMethodPointer ___m_defaultContextAction;
};
struct AsyncMethodBuilderCore_tD5ABB3A2536319A3345B32A5481E37E23DD8CEDF_marshaled_com
{
	RuntimeObject* ___m_stateMachine;
	Il2CppMethodPointer ___m_defaultContextAction;
};
struct Boolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22 
{
	bool ___m_value;
};
struct BypassElement_t1626DF2ACB357F8677EA94142CFA984569ADB4DC  : public ConfigurationElement_tAE3EE71C256825472831FFBB7F491275DFAF089E
{
};
struct Byte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3 
{
	uint8_t ___m_value;
};
struct CancellationToken_t51142D9C6D7C02D314DA34A6A7988C528992FFED 
{
	CancellationTokenSource_tAAE1E0033BCFC233801F8CB4CED5C852B350CB7B* ____source;
};
struct CancellationToken_t51142D9C6D7C02D314DA34A6A7988C528992FFED_marshaled_pinvoke
{
	CancellationTokenSource_tAAE1E0033BCFC233801F8CB4CED5C852B350CB7B* ____source;
};
struct CancellationToken_t51142D9C6D7C02D314DA34A6A7988C528992FFED_marshaled_com
{
	CancellationTokenSource_tAAE1E0033BCFC233801F8CB4CED5C852B350CB7B* ____source;
};
struct Char_t521A6F19B456D956AF452D926C32709DC03D6B17 
{
	Il2CppChar ___m_value;
};
struct ConfigurationElementCollection_t56E8398661A85A59616301BADF13026FB1492606  : public ConfigurationElement_tAE3EE71C256825472831FFBB7F491275DFAF089E
{
};
struct ConfigurationSection_t0BC609F0151B160A4FAB8226679B62AF22539C3E  : public ConfigurationElement_tAE3EE71C256825472831FFBB7F491275DFAF089E
{
};
struct ConnectionManagementElement_t44321336DB0AC70D7B85FAB146D7F66036D51353  : public ConfigurationElement_tAE3EE71C256825472831FFBB7F491275DFAF089E
{
};
struct DateTime_t66193957C73913903DDAD89FEDC46139BCA5802D 
{
	uint64_t ____dateData;
};
struct Double_tE150EF3D1D43DEE85D533810AB4C742307EEDE5F 
{
	double ___m_value;
};
struct Enum_t2A1A94B24E3B776EEF4E5E485E290BB9D4D072E2  : public ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F
{
};
struct Enum_t2A1A94B24E3B776EEF4E5E485E290BB9D4D072E2_marshaled_pinvoke
{
};
struct Enum_t2A1A94B24E3B776EEF4E5E485E290BB9D4D072E2_marshaled_com
{
};
struct HttpWebRequestElement_t8D2FEEC531D1FBE1B6E3D1077350A1EBDCB77DC3  : public ConfigurationElement_tAE3EE71C256825472831FFBB7F491275DFAF089E
{
};
struct Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C 
{
	int32_t ___m_value;
};
struct Int64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3 
{
	int64_t ___m_value;
};
struct IntPtr_t 
{
	void* ___m_value;
};
struct Ipv6Element_t480208D0E23E612DC1FD1CE19D9FB491F0F66932  : public ConfigurationElement_tAE3EE71C256825472831FFBB7F491275DFAF089E
{
};
struct MobileTlsProvider_tD60D82BEBF267F50F388A026DBB092C7188BB017  : public MonoTlsProvider_t39C898CDC9458EEAD7C019B4B23701EAF9E24F7E
{
};
struct NetSectionGroup_tA83DCAF89773087D0E921B92F67441132B71D52F  : public ConfigurationSectionGroup_tE7948C2D31B193F4BA8828947ED3094B952C7863
{
};
struct PerformanceCountersElement_tCB62C8CA7C218E35653DC5BB690A5FFD6D608B30  : public ConfigurationElement_tAE3EE71C256825472831FFBB7F491275DFAF089E
{
};
struct ProxyElement_t4D277820F9C77333BE8CC6498DE7E06C36868C65  : public ConfigurationElement_tAE3EE71C256825472831FFBB7F491275DFAF089E
{
};
struct ServicePointManagerElement_t98ED8374270537E77E57D014ECEE8B1A3A8EEFF2  : public ConfigurationElement_tAE3EE71C256825472831FFBB7F491275DFAF089E
{
};
struct SocketElement_tDC83FCE3C280D8D590972F989292C30E2B7F6CC6  : public ConfigurationElement_tAE3EE71C256825472831FFBB7F491275DFAF089E
{
};
struct Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE  : public MarshalByRefObject_t8C2F4C5854177FD60439EB1FCCFC1B3CFAFE8DCE
{
	ReadWriteTask_t0821BF49EE38596C7734E86E1A6A39D769BE2C05* ____activeReadWriteTask;
	SemaphoreSlim_t0D5CB5685D9BFA5BF95CEC6E7395490F933E8DB2* ____asyncActiveSemaphore;
};
struct TimeSpan_t8195C5B013A2C532FEBDF0B64B6911982E750F5A 
{
	int64_t ____ticks;
};
struct UInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B 
{
	uint32_t ___m_value;
};
struct Void_t4861ACF8F4594C3437BB48B6E56783494B843915 
{
	union
	{
		struct
		{
		};
		uint8_t Void_t4861ACF8F4594C3437BB48B6E56783494B843915__padding[1];
	};
};
struct WebProxyScriptElement_t0C0E2338F7F42D5D386F5AE68FF7F870FA6E7167  : public ConfigurationElement_tAE3EE71C256825472831FFBB7F491275DFAF089E
{
};
struct WebRequestModuleElement_t9856A4B94D713299F842F672DFD60557C38349FD  : public ConfigurationElement_tAE3EE71C256825472831FFBB7F491275DFAF089E
{
};
struct X509CertificateCollection_t2900D71D188EDCA7DEB5077D36103EE5DA6805CE  : public CollectionBase_t44F966CC555C87F2815D668FB4586526E1C2383F
{
};
#pragma pack(push, tp, 1)
struct __StaticArrayInitTypeSizeU3D10_t92CF09DC4E69422B6E8E804C0DE84FDE8FB89800 
{
	union
	{
		struct
		{
			union
			{
			};
		};
		uint8_t __StaticArrayInitTypeSizeU3D10_t92CF09DC4E69422B6E8E804C0DE84FDE8FB89800__padding[10];
	};
};
#pragma pack(pop, tp)
#pragma pack(push, tp, 1)
struct __StaticArrayInitTypeSizeU3D12_t32C4FAAB522AC6551334F4E397A28A8CF95552DD 
{
	union
	{
		struct
		{
			union
			{
			};
		};
		uint8_t __StaticArrayInitTypeSizeU3D12_t32C4FAAB522AC6551334F4E397A28A8CF95552DD__padding[12];
	};
};
#pragma pack(pop, tp)
#pragma pack(push, tp, 1)
struct __StaticArrayInitTypeSizeU3D128_tCC4BD0E4F5977323EDA6FFF6CD1484F1848F5B0E 
{
	union
	{
		struct
		{
			union
			{
			};
		};
		uint8_t __StaticArrayInitTypeSizeU3D128_tCC4BD0E4F5977323EDA6FFF6CD1484F1848F5B0E__padding[128];
	};
};
#pragma pack(pop, tp)
#pragma pack(push, tp, 1)
struct __StaticArrayInitTypeSizeU3D14_t5A7362C844CE0A2E27C9434CD029FA82339CFF26 
{
	union
	{
		struct
		{
			union
			{
			};
		};
		uint8_t __StaticArrayInitTypeSizeU3D14_t5A7362C844CE0A2E27C9434CD029FA82339CFF26__padding[14];
	};
};
#pragma pack(pop, tp)
#pragma pack(push, tp, 1)
struct __StaticArrayInitTypeSizeU3D256_tD401FB34D66ACCFAD94A32F8001469D038F350C8 
{
	union
	{
		struct
		{
			union
			{
			};
		};
		uint8_t __StaticArrayInitTypeSizeU3D256_tD401FB34D66ACCFAD94A32F8001469D038F350C8__padding[256];
	};
};
#pragma pack(pop, tp)
#pragma pack(push, tp, 1)
struct __StaticArrayInitTypeSizeU3D32_t543FA0727A92B3B7D8A0EB86D73AB8F35AB0CF48 
{
	union
	{
		struct
		{
			union
			{
			};
		};
		uint8_t __StaticArrayInitTypeSizeU3D32_t543FA0727A92B3B7D8A0EB86D73AB8F35AB0CF48__padding[32];
	};
};
#pragma pack(pop, tp)
#pragma pack(push, tp, 1)
struct __StaticArrayInitTypeSizeU3D44_tDBE82A359520A01FA0C117855F1C3185BA1AEB0F 
{
	union
	{
		struct
		{
			union
			{
			};
		};
		uint8_t __StaticArrayInitTypeSizeU3D44_tDBE82A359520A01FA0C117855F1C3185BA1AEB0F__padding[44];
	};
};
#pragma pack(pop, tp)
#pragma pack(push, tp, 1)
struct __StaticArrayInitTypeSizeU3D6_tA4881AF2477A5E66902A636B307315D4FA964D8D 
{
	union
	{
		struct
		{
			union
			{
			};
		};
		uint8_t __StaticArrayInitTypeSizeU3D6_tA4881AF2477A5E66902A636B307315D4FA964D8D__padding[6];
	};
};
#pragma pack(pop, tp)
#pragma pack(push, tp, 1)
struct __StaticArrayInitTypeSizeU3D9_tE2A1104C752B51004E61139D4615CC263948C4A7 
{
	union
	{
		struct
		{
			union
			{
			};
		};
		uint8_t __StaticArrayInitTypeSizeU3D9_tE2A1104C752B51004E61139D4615CC263948C4A7__padding[9];
	};
};
#pragma pack(pop, tp)
struct AsyncTaskMethodBuilder_1_tE810F083929D7952F192036D298085BD4B048AD0 
{
	AsyncMethodBuilderCore_tD5ABB3A2536319A3345B32A5481E37E23DD8CEDF ___m_coreState;
	Task_1_t0C4CD3A5BB93A184420D73218644C56C70FDA7E2* ___m_task;
};
struct AsyncTaskMethodBuilder_1_tA28B0962049DC249BEC0054E2394B1962E28E90E 
{
	AsyncMethodBuilderCore_tD5ABB3A2536319A3345B32A5481E37E23DD8CEDF ___m_coreState;
	Task_1_t3D7638C82ED289AF156EDBAE76842D8DF4C4A9E0* ___m_task;
};
struct ConfiguredTaskAwaitable_1_t5186C81524388C1718E9AC37792D8771A443417A 
{
	ConfiguredTaskAwaiter_t8FC486401D1951F2426478F2050EE290197B6E80 ___m_configuredTaskAwaiter;
};
struct Nullable_1_tEADC262F7F8B8BC4CC0A003DBDD3CA7C1B63F9AC 
{
	bool ___hasValue;
	DateTime_t66193957C73913903DDAD89FEDC46139BCA5802D ___value;
};
struct AuthenticatedStream_t8DCF41E151F705E2494FC7836F5E2EF7C539FA39  : public Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE
{
	Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* ____InnerStream;
	bool ____LeaveStreamOpen;
};
struct AuthenticationLevel_tD91F6CE700057352B4F45FC290E35B9E936DECAF 
{
	int32_t ___value__;
};
struct BypassElementCollection_t15465BC07F8DC8BABA6953182904AF762D638DBE  : public ConfigurationElementCollection_t56E8398661A85A59616301BADF13026FB1492606
{
};
struct ConnectionManagementElementCollection_tEEB0BEE8289364CCA1D76DDA3F862429612DE0F1  : public ConfigurationElementCollection_t56E8398661A85A59616301BADF13026FB1492606
{
};
struct ConnectionManagementSection_t8F4D45BE147F24F54F2124CC800B4594031F41C2  : public ConfigurationSection_t0BC609F0151B160A4FAB8226679B62AF22539C3E
{
};
struct DefaultProxySection_tC0247A534955DC9D887B03AA4EFB1C1E01AD1E34  : public ConfigurationSection_t0BC609F0151B160A4FAB8226679B62AF22539C3E
{
};
struct Delegate_t  : public RuntimeObject
{
	intptr_t ___method_ptr;
	intptr_t ___invoke_impl;
	RuntimeObject* ___m_target;
	intptr_t ___method;
	intptr_t ___delegate_trampoline;
	intptr_t ___extra_arg;
	intptr_t ___method_code;
	intptr_t ___interp_method;
	intptr_t ___interp_invoke_impl;
	MethodInfo_t* ___method_info;
	MethodInfo_t* ___original_method_info;
	DelegateData_t9B286B493293CD2D23A5B2B5EF0E5B1324C2B77E* ___data;
	bool ___method_is_virtual;
};
struct Delegate_t_marshaled_pinvoke
{
	intptr_t ___method_ptr;
	intptr_t ___invoke_impl;
	Il2CppIUnknown* ___m_target;
	intptr_t ___method;
	intptr_t ___delegate_trampoline;
	intptr_t ___extra_arg;
	intptr_t ___method_code;
	intptr_t ___interp_method;
	intptr_t ___interp_invoke_impl;
	MethodInfo_t* ___method_info;
	MethodInfo_t* ___original_method_info;
	DelegateData_t9B286B493293CD2D23A5B2B5EF0E5B1324C2B77E* ___data;
	int32_t ___method_is_virtual;
};
struct Delegate_t_marshaled_com
{
	intptr_t ___method_ptr;
	intptr_t ___invoke_impl;
	Il2CppIUnknown* ___m_target;
	intptr_t ___method;
	intptr_t ___delegate_trampoline;
	intptr_t ___extra_arg;
	intptr_t ___method_code;
	intptr_t ___interp_method;
	intptr_t ___interp_invoke_impl;
	MethodInfo_t* ___method_info;
	MethodInfo_t* ___original_method_info;
	DelegateData_t9B286B493293CD2D23A5B2B5EF0E5B1324C2B77E* ___data;
	int32_t ___method_is_virtual;
};
struct EncryptionPolicy_t5BCDD1A5A1B42E3843DBD8B55A1BECBD2A523D99 
{
	int32_t ___value__;
};
struct Exception_t  : public RuntimeObject
{
	String_t* ____className;
	String_t* ____message;
	RuntimeObject* ____data;
	Exception_t* ____innerException;
	String_t* ____helpURL;
	RuntimeObject* ____stackTrace;
	String_t* ____stackTraceString;
	String_t* ____remoteStackTraceString;
	int32_t ____remoteStackIndex;
	RuntimeObject* ____dynamicMethods;
	int32_t ____HResult;
	String_t* ____source;
	SafeSerializationManager_tCBB85B95DFD1634237140CD892E82D06ECB3F5E6* ____safeSerializationManager;
	StackTraceU5BU5D_t32FBCB20930EAF5BAE3F450FF75228E5450DA0DF* ___captured_traces;
	IntPtrU5BU5D_tFD177F8C806A6921AD7150264CCC62FA00CAD832* ___native_trace_ips;
	int32_t ___caught_in_unmanaged;
};
struct Exception_t_marshaled_pinvoke
{
	char* ____className;
	char* ____message;
	RuntimeObject* ____data;
	Exception_t_marshaled_pinvoke* ____innerException;
	char* ____helpURL;
	Il2CppIUnknown* ____stackTrace;
	char* ____stackTraceString;
	char* ____remoteStackTraceString;
	int32_t ____remoteStackIndex;
	Il2CppIUnknown* ____dynamicMethods;
	int32_t ____HResult;
	char* ____source;
	SafeSerializationManager_tCBB85B95DFD1634237140CD892E82D06ECB3F5E6* ____safeSerializationManager;
	StackTraceU5BU5D_t32FBCB20930EAF5BAE3F450FF75228E5450DA0DF* ___captured_traces;
	Il2CppSafeArray* ___native_trace_ips;
	int32_t ___caught_in_unmanaged;
};
struct Exception_t_marshaled_com
{
	Il2CppChar* ____className;
	Il2CppChar* ____message;
	RuntimeObject* ____data;
	Exception_t_marshaled_com* ____innerException;
	Il2CppChar* ____helpURL;
	Il2CppIUnknown* ____stackTrace;
	Il2CppChar* ____stackTraceString;
	Il2CppChar* ____remoteStackTraceString;
	int32_t ____remoteStackIndex;
	Il2CppIUnknown* ____dynamicMethods;
	int32_t ____HResult;
	Il2CppChar* ____source;
	SafeSerializationManager_tCBB85B95DFD1634237140CD892E82D06ECB3F5E6* ____safeSerializationManager;
	StackTraceU5BU5D_t32FBCB20930EAF5BAE3F450FF75228E5450DA0DF* ___captured_traces;
	Il2CppSafeArray* ___native_trace_ips;
	int32_t ___caught_in_unmanaged;
};
struct SeekOrigin_t7EB9AD0EDF26368A40F48FA2098F02160B1E8000 
{
	int32_t ___value__;
};
struct SettingsSection_tC931BA69B22D5E0ED07698F02778482987319A12  : public ConfigurationSection_t0BC609F0151B160A4FAB8226679B62AF22539C3E
{
};
struct SslPolicyErrors_t92DF45EC5BA5BEFF38EF53C2C8488B27385EBB08 
{
	int32_t ___value__;
};
struct SslProtocols_t21FADB874FCAEC5039AE593AA3544639C938C77E 
{
	int32_t ___value__;
};
struct StreamingContextStates_t5EE358E619B251608A9327618C7BFE8638FC33C1 
{
	int32_t ___value__;
};
struct TlsProtocols_tCC009DAEEFCA5769448145AD24A1193CD5E8541B 
{
	int32_t ___value__;
};
struct WebRequestModuleElementCollection_t5979A19A48B822831C1D351EB2AA25058C7F7463  : public ConfigurationElementCollection_t56E8398661A85A59616301BADF13026FB1492606
{
};
struct WebRequestModulesSection_t4099F9890D7851314C5528C577E4C51B36C97D00  : public ConfigurationSection_t0BC609F0151B160A4FAB8226679B62AF22539C3E
{
};
struct WebSocketCloseStatus_tED432212F88FCCA42F59AAA6BCF82962A2CFF4DF 
{
	int32_t ___value__;
};
struct WebSocketError_tEC1353C241C6481FE77428ABEB57E39B8E6E7672 
{
	int32_t ___value__;
};
struct WebSocketMessageType_t7EA078C3F0E1D384B2F3B09BDA8CFD61406A9EC9 
{
	int32_t ___value__;
};
struct WebSocketState_tDA525C32450C7EA01B5D22DAB7A6DAB08AADBF25 
{
	int32_t ___value__;
};
struct X509Certificate_t966CC553AF25AE7991F5B4C2AACBCF6C66C8F9C4  : public RuntimeObject
{
	X509CertificateImpl_tF590E81705CE1FE152C590E5A875D4FE3BE348EF* ___impl;
	ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* ___lazyCertHash;
	ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* ___lazySerialNumber;
	String_t* ___lazyIssuer;
	String_t* ___lazySubject;
	String_t* ___lazyKeyAlgorithm;
	ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* ___lazyKeyAlgorithmParameters;
	ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* ___lazyPublicKey;
	DateTime_t66193957C73913903DDAD89FEDC46139BCA5802D ___lazyNotBefore;
	DateTime_t66193957C73913903DDAD89FEDC46139BCA5802D ___lazyNotAfter;
};
struct X509RevocationMode_t952728D003111036C0DF94B0F66FF02B7DB04E62 
{
	int32_t ___value__;
};
struct MessageOpcode_t1C95E00757D25B320970B2BAF415B7537D337C18 
{
	uint8_t ___value__;
};
struct Operation_tC35F3423442F066D3B3CB2E5FCC61CEA2D618017 
{
	int32_t ___value__;
};
struct Nullable_1_t9A98093485034F2B86BC66B725022122E0E5B2A4 
{
	bool ___hasValue;
	int32_t ___value;
};
struct Nullable_1_t4936015D54B4E5C3191E452324F5F52BD55E1284 
{
	bool ___hasValue;
	int32_t ___value;
};
struct MobileAuthenticatedStream_tD0306DC2B0CDA3C7DB261C19FFA35CA8EE24309E  : public AuthenticatedStream_t8DCF41E151F705E2494FC7836F5E2EF7C539FA39
{
	MobileTlsContext_t456DDC89866EE34EF32EEF959AD92C9F17684476* ___xobileTlsContext;
	ExceptionDispatchInfo_tD7AF19E75FEC22F4A8329FD1E9EDF96615CB2757* ___lastException;
	AsyncProtocolRequest_tF31CF30F8C57A3D5DC10A9FB0A5568FA254DFE86* ___asyncHandshakeRequest;
	AsyncProtocolRequest_tF31CF30F8C57A3D5DC10A9FB0A5568FA254DFE86* ___asyncReadRequest;
	AsyncProtocolRequest_tF31CF30F8C57A3D5DC10A9FB0A5568FA254DFE86* ___asyncWriteRequest;
	BufferOffsetSize2_t2C3F7F42F64D84F357DC24AFCD44AC4679B20629* ___readBuffer;
	BufferOffsetSize2_t2C3F7F42F64D84F357DC24AFCD44AC4679B20629* ___writeBuffer;
	RuntimeObject* ___ioLock;
	int32_t ___closeRequested;
	bool ___shutdown;
	int32_t ___operation;
	SslStream_t19A079881850F9CAD7BAA6FB625BBC4647ED5A27* ___U3CSslStreamU3Ek__BackingField;
	MonoTlsSettings_tD79AF4AE5C2CD533A3D7A08FED479B1EC1A031B0* ___U3CSettingsU3Ek__BackingField;
	MobileTlsProvider_tD60D82BEBF267F50F388A026DBB092C7188BB017* ___U3CProviderU3Ek__BackingField;
	String_t* ___U3CTargetHostU3Ek__BackingField;
	int32_t ___ID;
};
struct MulticastDelegate_t  : public Delegate_t
{
	DelegateU5BU5D_tC5AB7E8F745616680F337909D3A8E6C722CDF771* ___delegates;
};
struct MulticastDelegate_t_marshaled_pinvoke : public Delegate_t_marshaled_pinvoke
{
	Delegate_t_marshaled_pinvoke** ___delegates;
};
struct MulticastDelegate_t_marshaled_com : public Delegate_t_marshaled_com
{
	Delegate_t_marshaled_com** ___delegates;
};
struct SslClientAuthenticationOptions_tA5277DAACAFB33887D2513D9399C5154B0D5CBC9  : public RuntimeObject
{
	int32_t ____encryptionPolicy;
	int32_t ____checkCertificateRevocation;
	int32_t ____enabledSslProtocols;
	bool ____allowRenegotiation;
	String_t* ___U3CTargetHostU3Ek__BackingField;
	X509CertificateCollection_t2900D71D188EDCA7DEB5077D36103EE5DA6805CE* ___U3CClientCertificatesU3Ek__BackingField;
};
struct SslStream_t19A079881850F9CAD7BAA6FB625BBC4647ED5A27  : public AuthenticatedStream_t8DCF41E151F705E2494FC7836F5E2EF7C539FA39
{
	MobileTlsProvider_tD60D82BEBF267F50F388A026DBB092C7188BB017* ___provider;
	MonoTlsSettings_tD79AF4AE5C2CD533A3D7A08FED479B1EC1A031B0* ___settings;
	MobileAuthenticatedStream_tD0306DC2B0CDA3C7DB261C19FFA35CA8EE24309E* ___impl;
};
struct StreamingContext_t56760522A751890146EE45F82F866B55B7E33677 
{
	RuntimeObject* ___m_additionalContext;
	int32_t ___m_state;
};
struct StreamingContext_t56760522A751890146EE45F82F866B55B7E33677_marshaled_pinvoke
{
	Il2CppIUnknown* ___m_additionalContext;
	int32_t ___m_state;
};
struct StreamingContext_t56760522A751890146EE45F82F866B55B7E33677_marshaled_com
{
	Il2CppIUnknown* ___m_additionalContext;
	int32_t ___m_state;
};
struct SystemException_tCC48D868298F4C0705279823E34B00F4FBDB7295  : public Exception_t
{
};
struct WebSocketHandle_t9C8D33BD3D8553B1FD363A68ABDF5B81FC089F50  : public RuntimeObject
{
	CancellationTokenSource_tAAE1E0033BCFC233801F8CB4CED5C852B350CB7B* ____abortSource;
	int32_t ____state;
	WebSocket_tE3DEAFF0F68BD5AF9A526D29D56B6E5C6E24C3A1* ____webSocket;
};
struct MessageHeader_tE18ECF225BF4676569B3F11CD425FF3EF6DF80F0 
{
	uint8_t ___Opcode;
	bool ___Fin;
	int64_t ___PayloadLength;
	int32_t ___Mask;
};
struct U3CReadResponseHeaderLineAsyncU3Ed__32_tEEAA4B9F7D1F99D0F6B270421640B301B419757B 
{
	int32_t ___U3CU3E1__state;
	AsyncTaskMethodBuilder_1_tA28B0962049DC249BEC0054E2394B1962E28E90E ___U3CU3Et__builder;
	Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* ___stream;
	CancellationToken_t51142D9C6D7C02D314DA34A6A7988C528992FFED ___cancellationToken;
	StringBuilder_t* ___U3CsbU3E5__2;
	ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* ___U3CarrU3E5__3;
	Il2CppChar ___U3CprevCharU3E5__4;
	ConfiguredTaskAwaiter_t8FC486401D1951F2426478F2050EE290197B6E80 ___U3CU3Eu__1;
};
struct ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263  : public SystemException_tCC48D868298F4C0705279823E34B00F4FBDB7295
{
	String_t* ____paramName;
};
struct AsyncCallback_t7FEF460CBDCFB9C5FA2EF776984778B9A4145F4C  : public MulticastDelegate_t
{
};
struct ExternalException_t419875A3CD3C551692EDBBC99E4927E69F2E1F4C  : public SystemException_tCC48D868298F4C0705279823E34B00F4FBDB7295
{
};
struct InvalidOperationException_t5DDE4D49B7405FAAB1E4576F4715A42A3FAD4BAB  : public SystemException_tCC48D868298F4C0705279823E34B00F4FBDB7295
{
};
struct LocalCertSelectionCallback_t3AB79A8AA642074A3EF886118963EEA3CCABABE2  : public MulticastDelegate_t
{
};
struct ManagedWebSocket_tFE006AE2E3A7EFDD703AAA7AC6FC035F65695F58  : public WebSocket_tE3DEAFF0F68BD5AF9A526D29D56B6E5C6E24C3A1
{
	Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* ____stream;
	bool ____isServer;
	String_t* ____subprotocol;
	Timer_t763C1D5F5A36087DC92C7DA4D1F8AB578F83AB00* ____keepAliveTimer;
	CancellationTokenSource_tAAE1E0033BCFC233801F8CB4CED5C852B350CB7B* ____abortSource;
	Memory_1_tB7CEF4416F5014E364267478CEF016A4AC5C0036 ____receiveBuffer;
	Utf8MessageState_tAC717AAC548FCA700FE47E9DF4FFCC61E55AF91D* ____utf8TextState;
	SemaphoreSlim_t0D5CB5685D9BFA5BF95CEC6E7395490F933E8DB2* ____sendFrameAsyncLock;
	int32_t ____state;
	bool ____disposed;
	bool ____sentCloseFrame;
	bool ____receivedCloseFrame;
	Nullable_1_t4936015D54B4E5C3191E452324F5F52BD55E1284 ____closeStatus;
	String_t* ____closeStatusDescription;
	MessageHeader_tE18ECF225BF4676569B3F11CD425FF3EF6DF80F0 ____lastReceiveHeader;
	int32_t ____receiveBufferOffset;
	int32_t ____receiveBufferCount;
	int32_t ____receivedMaskOffsetOffset;
	ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* ____sendBuffer;
	Task_t751C4CC3ECD055BABA8A0B6A5DFBB4283DCA8572* ____lastReceiveAsync;
};
struct MonoTlsSettings_tD79AF4AE5C2CD533A3D7A08FED479B1EC1A031B0  : public RuntimeObject
{
	MonoRemoteCertificateValidationCallback_t1A389B61998873F6B9A2EE7A11C36333A8AECCA0* ___U3CRemoteCertificateValidationCallbackU3Ek__BackingField;
	MonoLocalCertificateSelectionCallback_t34F7772BA5ECE38E6CBD4C311F579DD1D4724DE3* ___U3CClientCertificateSelectionCallbackU3Ek__BackingField;
	Nullable_1_tEADC262F7F8B8BC4CC0A003DBDD3CA7C1B63F9AC ___U3CCertificateValidationTimeU3Ek__BackingField;
	X509CertificateCollection_t2900D71D188EDCA7DEB5077D36103EE5DA6805CE* ___U3CTrustAnchorsU3Ek__BackingField;
	RuntimeObject* ___U3CUserSettingsU3Ek__BackingField;
	StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* ___U3CCertificateSearchPathsU3Ek__BackingField;
	bool ___U3CSendCloseNotifyU3Ek__BackingField;
	StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* ___U3CClientCertificateIssuersU3Ek__BackingField;
	bool ___U3CDisallowUnauthenticatedCertificateRequestU3Ek__BackingField;
	Nullable_1_t9A98093485034F2B86BC66B725022122E0E5B2A4 ___U3CEnabledProtocolsU3Ek__BackingField;
	CipherSuiteCodeU5BU5D_t61EC0E6F53394985FFC36DEB587C70F4EE26D435* ___U3CEnabledCiphersU3Ek__BackingField;
	bool ___cloned;
	bool ___checkCertName;
	bool ___checkCertRevocationStatus;
	Nullable_1_t78F453FADB4A9F50F267A4E349019C34410D1A01 ___useServicePointManagerCallback;
	bool ___skipSystemValidators;
	bool ___callbackNeedsChain;
	RuntimeObject* ___certificateValidator;
};
struct NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A  : public SystemException_tCC48D868298F4C0705279823E34B00F4FBDB7295
{
};
struct RemoteCertificateValidationCallback_t2F4C5801F96B2C2BF934511796C5BFEAEBF01955  : public MulticastDelegate_t
{
};
struct ServerCertSelectionCallback_t653386CAEAE0236FCF61A92963AB1646BB23C654  : public MulticastDelegate_t
{
};
struct WebSocketReceiveResult_t31FCE9F2E53843C065CD57BE10581EE47D6FAD2C  : public RuntimeObject
{
	int32_t ___U3CCountU3Ek__BackingField;
	bool ___U3CEndOfMessageU3Ek__BackingField;
	int32_t ___U3CMessageTypeU3Ek__BackingField;
	Nullable_1_t4936015D54B4E5C3191E452324F5F52BD55E1284 ___U3CCloseStatusU3Ek__BackingField;
	String_t* ___U3CCloseStatusDescriptionU3Ek__BackingField;
};
struct ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129  : public ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263
{
};
struct ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F  : public ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263
{
	RuntimeObject* ____actualValue;
};
struct ObjectDisposedException_tC5FB29E8E980E2010A2F6A5B9B791089419F89EB  : public InvalidOperationException_t5DDE4D49B7405FAAB1E4576F4715A42A3FAD4BAB
{
	String_t* ____objectName;
};
struct PlatformNotSupportedException_tD2BD7EB9278518AA5FE8AE75AD5D0D4298A4631A  : public NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A
{
};
struct Win32Exception_t15A75629914EB77C816D8219D93ED78E50C74BE9  : public ExternalException_t419875A3CD3C551692EDBBC99E4927E69F2E1F4C
{
	int32_t ___nativeErrorCode;
};
struct ThrowStub_tA028CA7941154A8E0F17F9AD0563D538617A7A47  : public ObjectDisposedException_tC5FB29E8E980E2010A2F6A5B9B791089419F89EB
{
};
struct WebSocketException_t7ED58EA50E372ED89E1469A44379C2FB097A6263  : public Win32Exception_t15A75629914EB77C816D8219D93ED78E50C74BE9
{
	int32_t ____webSocketErrorCode;
};
struct U3CPrivateImplementationDetailsU3E_t56F624E1051A2E261613B6A81CA2333397F49CB3_StaticFields
{
	__StaticArrayInitTypeSizeU3D256_tD401FB34D66ACCFAD94A32F8001469D038F350C8 ___00C700F38385659BA060672F86D4A9A5376EADF9ED1CABB1C63290A0FDEFE36A;
	__StaticArrayInitTypeSizeU3D32_t543FA0727A92B3B7D8A0EB86D73AB8F35AB0CF48 ___04D9D643E5A26924DC98C9906714082D395DCB17CEE9674D5EBFEC15D098C2B5;
	__StaticArrayInitTypeSizeU3D32_t543FA0727A92B3B7D8A0EB86D73AB8F35AB0CF48 ___063691760A57230E8119DF650327304D68AE1C009E476E48BE8C73DCA199D374;
	__StaticArrayInitTypeSizeU3D14_t5A7362C844CE0A2E27C9434CD029FA82339CFF26 ___07FEA8B695D401E2B3BBDA6126C123DAA87153BB12384B21272ACC90823AC9A3;
	int64_t ___1417FCAA8455DC8E3C88BF916DD5C75746A2597CC052F212A8E4394C890E012B;
	__StaticArrayInitTypeSizeU3D9_tE2A1104C752B51004E61139D4615CC263948C4A7 ___27625E383C3A91E8B65BC745FF5D4048C82B883CCD293B07DED697BF82733811;
	__StaticArrayInitTypeSizeU3D32_t543FA0727A92B3B7D8A0EB86D73AB8F35AB0CF48 ___2EF83B43314F8CD03190EEE30ECCF048DA37791237F27C62A579F23EACE9FD70;
	int64_t ___3505B8A2248AC03FE41ACADF8F29294572BBADEE1DD2E1A45D025766681C012C;
	__StaticArrayInitTypeSizeU3D12_t32C4FAAB522AC6551334F4E397A28A8CF95552DD ___356A582FCE68ACAE8FC944F7B4F0C0DA2BFF4A90D3767C75905F3EDE2E4E66CF;
	__StaticArrayInitTypeSizeU3D9_tE2A1104C752B51004E61139D4615CC263948C4A7 ___684312AFB7719E57993D2826FFBAF7EA965614F20F91D999FB19B01E21AA62E6;
	__StaticArrayInitTypeSizeU3D128_tCC4BD0E4F5977323EDA6FFF6CD1484F1848F5B0E ___8109EF063456779751E8A4AEE94A0BE9AE32827B6EB153ABD7F40057FF2C9CE3;
	__StaticArrayInitTypeSizeU3D44_tDBE82A359520A01FA0C117855F1C3185BA1AEB0F ___9001C3EDE1D7E939C07996B84F82F7CBE59A7266C1FC2735E959E3C8609602E8;
	__StaticArrayInitTypeSizeU3D12_t32C4FAAB522AC6551334F4E397A28A8CF95552DD ___9DB0F923DCA529159AFF809431DDCC62C801C60246F66B693B861CBF79292994;
	__StaticArrayInitTypeSizeU3D6_tA4881AF2477A5E66902A636B307315D4FA964D8D ___9F58F1261D211553DE8FBC1AFA477C84EA3974B9BFF38223D771D0CDA787E15A;
	__StaticArrayInitTypeSizeU3D128_tCC4BD0E4F5977323EDA6FFF6CD1484F1848F5B0E ___B849624EC20707184A433E21DAAC963906D4B261A19BD8F139F0E269E59E308C;
	__StaticArrayInitTypeSizeU3D14_t5A7362C844CE0A2E27C9434CD029FA82339CFF26 ___DFA7289CF8D8029ACEE90F30530C6926950E8DD20285772458F271AFD015BAD5;
	__StaticArrayInitTypeSizeU3D10_t92CF09DC4E69422B6E8E804C0DE84FDE8FB89800 ___E478CC6C0579E2198C99BFDE0ABAADC66644AF69312CB9D6E94E2D4E3559482A;
};
struct String_t_StaticFields
{
	String_t* ___Empty;
};
struct Task_t751C4CC3ECD055BABA8A0B6A5DFBB4283DCA8572_StaticFields
{
	int32_t ___s_taskIdCounter;
	RuntimeObject* ___s_taskCompletionSentinel;
	bool ___s_asyncDebuggingEnabled;
	Action_1_t6F9EB113EB3F16226AEF811A2744F4111C116C87* ___s_taskCancelCallback;
	Func_1_tD59A12717D79BFB403BF973694B1BE5B85474BD1* ___s_createContingentProperties;
	TaskFactory_tF781BD37BE23917412AD83424D1497C7C1509DF0* ___U3CFactoryU3Ek__BackingField;
	Task_t751C4CC3ECD055BABA8A0B6A5DFBB4283DCA8572* ___U3CCompletedTaskU3Ek__BackingField;
	Predicate_1_t7F48518B008C1472339EEEBABA3DE203FE1F26ED* ___s_IsExceptionObservedByParentPredicate;
	ContextCallback_tE8AFBDBFCC040FDA8DA8C1EEFE9BD66B16BDA007* ___s_ecCallback;
	Predicate_1_t8342C85FF4E41CD1F7024AC0CDC3E5312A32CB12* ___s_IsTaskContinuationNullPredicate;
	Dictionary_2_t403063CE4960B4F46C688912237C6A27E550FF55* ___s_currentActiveTasks;
	RuntimeObject* ___s_activeTasksLock;
};
struct Task_t751C4CC3ECD055BABA8A0B6A5DFBB4283DCA8572_ThreadStaticFields
{
	Task_t751C4CC3ECD055BABA8A0B6A5DFBB4283DCA8572* ___t_currentTask;
	StackGuard_tACE063A1B7374BDF4AD472DE4585D05AD8745352* ___t_stackGuard;
};
struct Timeout_tDBB45B56C59D1AF9A6B1EC5F1E3200D12AB18E41_StaticFields
{
	TimeSpan_t8195C5B013A2C532FEBDF0B64B6911982E750F5A ___InfiniteTimeSpan;
};
struct ArraySegment_1_t3DC888623B720A071D69279F1FCB95A109195093_StaticFields
{
	ArraySegment_1_t3DC888623B720A071D69279F1FCB95A109195093 ___U3CEmptyU3Ek__BackingField;
};
struct Task_1_t4C228DE57804012969575431CFF12D57C875552D_StaticFields
{
	TaskFactory_1_t0BEF06D58E44525B9135AB0B22D016856EE69FF3* ___s_defaultFactory;
};
struct Boolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_StaticFields
{
	String_t* ___TrueString;
	String_t* ___FalseString;
};
struct CancellationToken_t51142D9C6D7C02D314DA34A6A7988C528992FFED_StaticFields
{
	Action_1_t6F9EB113EB3F16226AEF811A2744F4111C116C87* ___s_actionToActionObjShunt;
};
struct Char_t521A6F19B456D956AF452D926C32709DC03D6B17_StaticFields
{
	ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* ___s_categoryForLatin1;
};
struct IntPtr_t_StaticFields
{
	intptr_t ___Zero;
};
struct Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE_StaticFields
{
	Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* ___Null;
};
struct TimeSpan_t8195C5B013A2C532FEBDF0B64B6911982E750F5A_StaticFields
{
	TimeSpan_t8195C5B013A2C532FEBDF0B64B6911982E750F5A ___Zero;
	TimeSpan_t8195C5B013A2C532FEBDF0B64B6911982E750F5A ___MaxValue;
	TimeSpan_t8195C5B013A2C532FEBDF0B64B6911982E750F5A ___MinValue;
};
struct AsyncTaskMethodBuilder_1_tE810F083929D7952F192036D298085BD4B048AD0_StaticFields
{
	Task_1_t0C4CD3A5BB93A184420D73218644C56C70FDA7E2* ___s_defaultResultTask;
};
struct AsyncTaskMethodBuilder_1_tA28B0962049DC249BEC0054E2394B1962E28E90E_StaticFields
{
	Task_1_t3D7638C82ED289AF156EDBAE76842D8DF4C4A9E0* ___s_defaultResultTask;
};
struct Exception_t_StaticFields
{
	RuntimeObject* ___s_EDILock;
};
struct MobileAuthenticatedStream_tD0306DC2B0CDA3C7DB261C19FFA35CA8EE24309E_StaticFields
{
	int32_t ___uniqueNameInteger;
	int32_t ___nextId;
};
struct WebSocketHandle_t9C8D33BD3D8553B1FD363A68ABDF5B81FC089F50_StaticFields
{
	Encoding_t65CDEF28CF20A7B8C92E85A4E808920C2465F095* ___s_defaultHttpEncoding;
};
struct WebSocketHandle_t9C8D33BD3D8553B1FD363A68ABDF5B81FC089F50_ThreadStaticFields
{
	StringBuilder_t* ___t_cachedStringBuilder;
};
struct ManagedWebSocket_tFE006AE2E3A7EFDD703AAA7AC6FC035F65695F58_StaticFields
{
	RandomNumberGenerator_t4E862666A2F7D55324960670C7A1B4C2D40222F3* ___s_random;
	UTF8Encoding_t90B56215A1B0B7ED5CDEA772E695F0DDAFBCD3BE* ___s_textEncoding;
	WebSocketStateU5BU5D_t3998D3C6855D91A27627D1D6135ADC884B2E4C1B* ___s_validSendStates;
	WebSocketStateU5BU5D_t3998D3C6855D91A27627D1D6135ADC884B2E4C1B* ___s_validReceiveStates;
	WebSocketStateU5BU5D_t3998D3C6855D91A27627D1D6135ADC884B2E4C1B* ___s_validCloseOutputStates;
	WebSocketStateU5BU5D_t3998D3C6855D91A27627D1D6135ADC884B2E4C1B* ___s_validCloseStates;
	Task_1_t7A5D3E6F94872974689076E6A3A64E3104677A40* ___s_cachedCloseTask;
};
struct MonoTlsSettings_tD79AF4AE5C2CD533A3D7A08FED479B1EC1A031B0_StaticFields
{
	MonoTlsSettings_tD79AF4AE5C2CD533A3D7A08FED479B1EC1A031B0* ___defaultSettings;
};
#ifdef __clang__
#pragma clang diagnostic pop
#endif
struct ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031  : public RuntimeArray
{
	ALIGN_FIELD (8) uint8_t m_Items[1];

	inline uint8_t GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline uint8_t* GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, uint8_t value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
	}
	inline uint8_t GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline uint8_t* GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, uint8_t value)
	{
		m_Items[index] = value;
	}
};
struct DelegateU5BU5D_tC5AB7E8F745616680F337909D3A8E6C722CDF771  : public RuntimeArray
{
	ALIGN_FIELD (8) Delegate_t* m_Items[1];

	inline Delegate_t* GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline Delegate_t** GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, Delegate_t* value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
	inline Delegate_t* GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline Delegate_t** GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, Delegate_t* value)
	{
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
};
struct StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248  : public RuntimeArray
{
	ALIGN_FIELD (8) String_t* m_Items[1];

	inline String_t* GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline String_t** GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, String_t* value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
	inline String_t* GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline String_t** GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, String_t* value)
	{
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
};


IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ConfiguredTaskAwaitable_1_t5186C81524388C1718E9AC37792D8771A443417A Task_1_ConfigureAwait_m8203F4D13209C12845066A383E5B850D0486B209_gshared (Task_1_t4C228DE57804012969575431CFF12D57C875552D* __this, bool ___0_continueOnCapturedContext, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR ConfiguredTaskAwaiter_t8FC486401D1951F2426478F2050EE290197B6E80 ConfiguredTaskAwaitable_1_GetAwaiter_m7A77B1981FEC19CC7E1570EDC3F16AC5C14B4439_gshared_inline (ConfiguredTaskAwaitable_1_t5186C81524388C1718E9AC37792D8771A443417A* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool ConfiguredTaskAwaiter_get_IsCompleted_mD70263ED42C8E379EE20DBC2F218C3E629B4B4D3_gshared (ConfiguredTaskAwaiter_t8FC486401D1951F2426478F2050EE290197B6E80* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AsyncTaskMethodBuilder_1_AwaitUnsafeOnCompleted_TisConfiguredTaskAwaiter_t8FC486401D1951F2426478F2050EE290197B6E80_TisU3CReadResponseHeaderLineAsyncU3Ed__32_tEEAA4B9F7D1F99D0F6B270421640B301B419757B_mB26AD0A4FF1BD2A4B2EB161CAC517A4507CEE572_gshared (AsyncTaskMethodBuilder_1_tE810F083929D7952F192036D298085BD4B048AD0* __this, ConfiguredTaskAwaiter_t8FC486401D1951F2426478F2050EE290197B6E80* ___0_awaiter, U3CReadResponseHeaderLineAsyncU3Ed__32_tEEAA4B9F7D1F99D0F6B270421640B301B419757B* ___1_stateMachine, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t ConfiguredTaskAwaiter_GetResult_mF1FF6CBD66A3F581D413793BA8C2AF58B707D9CC_gshared (ConfiguredTaskAwaiter_t8FC486401D1951F2426478F2050EE290197B6E80* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AsyncTaskMethodBuilder_1_SetException_mC2F74B26F5303F9F960965220E2866D777F1A5C6_gshared (AsyncTaskMethodBuilder_1_tE810F083929D7952F192036D298085BD4B048AD0* __this, Exception_t* ___0_exception, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AsyncTaskMethodBuilder_1_SetResult_m0D83195F995F9825D7A6DCDC3835D6917C43B5A6_gshared (AsyncTaskMethodBuilder_1_tE810F083929D7952F192036D298085BD4B048AD0* __this, RuntimeObject* ___0_result, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AsyncTaskMethodBuilder_1_SetStateMachine_m3BE54983634ABF5BE05200C7894AD0F9F20BDD65_gshared (AsyncTaskMethodBuilder_1_tE810F083929D7952F192036D298085BD4B048AD0* __this, RuntimeObject* ___0_stateMachine, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t TaskToApm_End_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mCA309E122256972A27C74FC814F47AD13AF623ED_gshared (RuntimeObject* ___0_asyncResult, const RuntimeMethod* method) ;

IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void StringBuilder__ctor_m1D99713357DE05DAFA296633639DB55F8C30587D (StringBuilder_t* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR StringBuilder_t* StringBuilder_Clear_m2D76F6533574F40A4E3E2DC4E730277CBD0AF8F6 (StringBuilder_t* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR StringBuilder_t* StringBuilder_Append_m71228B30F05724CD2CD96D9611DCD61BFB96A6E1 (StringBuilder_t* __this, Il2CppChar ___0_value, const RuntimeMethod* method) ;
inline ConfiguredTaskAwaitable_1_t5186C81524388C1718E9AC37792D8771A443417A Task_1_ConfigureAwait_m8203F4D13209C12845066A383E5B850D0486B209 (Task_1_t4C228DE57804012969575431CFF12D57C875552D* __this, bool ___0_continueOnCapturedContext, const RuntimeMethod* method)
{
	return ((  ConfiguredTaskAwaitable_1_t5186C81524388C1718E9AC37792D8771A443417A (*) (Task_1_t4C228DE57804012969575431CFF12D57C875552D*, bool, const RuntimeMethod*))Task_1_ConfigureAwait_m8203F4D13209C12845066A383E5B850D0486B209_gshared)(__this, ___0_continueOnCapturedContext, method);
}
inline ConfiguredTaskAwaiter_t8FC486401D1951F2426478F2050EE290197B6E80 ConfiguredTaskAwaitable_1_GetAwaiter_m7A77B1981FEC19CC7E1570EDC3F16AC5C14B4439_inline (ConfiguredTaskAwaitable_1_t5186C81524388C1718E9AC37792D8771A443417A* __this, const RuntimeMethod* method)
{
	return ((  ConfiguredTaskAwaiter_t8FC486401D1951F2426478F2050EE290197B6E80 (*) (ConfiguredTaskAwaitable_1_t5186C81524388C1718E9AC37792D8771A443417A*, const RuntimeMethod*))ConfiguredTaskAwaitable_1_GetAwaiter_m7A77B1981FEC19CC7E1570EDC3F16AC5C14B4439_gshared_inline)(__this, method);
}
inline bool ConfiguredTaskAwaiter_get_IsCompleted_mD70263ED42C8E379EE20DBC2F218C3E629B4B4D3 (ConfiguredTaskAwaiter_t8FC486401D1951F2426478F2050EE290197B6E80* __this, const RuntimeMethod* method)
{
	return ((  bool (*) (ConfiguredTaskAwaiter_t8FC486401D1951F2426478F2050EE290197B6E80*, const RuntimeMethod*))ConfiguredTaskAwaiter_get_IsCompleted_mD70263ED42C8E379EE20DBC2F218C3E629B4B4D3_gshared)(__this, method);
}
inline void AsyncTaskMethodBuilder_1_AwaitUnsafeOnCompleted_TisConfiguredTaskAwaiter_t8FC486401D1951F2426478F2050EE290197B6E80_TisU3CReadResponseHeaderLineAsyncU3Ed__32_tEEAA4B9F7D1F99D0F6B270421640B301B419757B_m43E8610E0472D376CD75485C575F5C9C8D91AE36 (AsyncTaskMethodBuilder_1_tA28B0962049DC249BEC0054E2394B1962E28E90E* __this, ConfiguredTaskAwaiter_t8FC486401D1951F2426478F2050EE290197B6E80* ___0_awaiter, U3CReadResponseHeaderLineAsyncU3Ed__32_tEEAA4B9F7D1F99D0F6B270421640B301B419757B* ___1_stateMachine, const RuntimeMethod* method)
{
	((  void (*) (AsyncTaskMethodBuilder_1_tA28B0962049DC249BEC0054E2394B1962E28E90E*, ConfiguredTaskAwaiter_t8FC486401D1951F2426478F2050EE290197B6E80*, U3CReadResponseHeaderLineAsyncU3Ed__32_tEEAA4B9F7D1F99D0F6B270421640B301B419757B*, const RuntimeMethod*))AsyncTaskMethodBuilder_1_AwaitUnsafeOnCompleted_TisConfiguredTaskAwaiter_t8FC486401D1951F2426478F2050EE290197B6E80_TisU3CReadResponseHeaderLineAsyncU3Ed__32_tEEAA4B9F7D1F99D0F6B270421640B301B419757B_mB26AD0A4FF1BD2A4B2EB161CAC517A4507CEE572_gshared)(__this, ___0_awaiter, ___1_stateMachine, method);
}
inline int32_t ConfiguredTaskAwaiter_GetResult_mF1FF6CBD66A3F581D413793BA8C2AF58B707D9CC (ConfiguredTaskAwaiter_t8FC486401D1951F2426478F2050EE290197B6E80* __this, const RuntimeMethod* method)
{
	return ((  int32_t (*) (ConfiguredTaskAwaiter_t8FC486401D1951F2426478F2050EE290197B6E80*, const RuntimeMethod*))ConfiguredTaskAwaiter_GetResult_mF1FF6CBD66A3F581D413793BA8C2AF58B707D9CC_gshared)(__this, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t StringBuilder_get_Length_mDEA041E7357C68CC3B5885276BB403676DAAE0D8 (StringBuilder_t* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppChar StringBuilder_get_Chars_m254FD6F2F75C00B0D353D73B2A4A19316BD7624D (StringBuilder_t* __this, int32_t ___0_index, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void StringBuilder_set_Length_mE2427BDAEF91C4E4A6C80F3BDF1F6E01DBCC2414 (StringBuilder_t* __this, int32_t ___0_value, const RuntimeMethod* method) ;
inline void AsyncTaskMethodBuilder_1_SetException_m897218C2DB8D7BA446EB263573085805B8F0638C (AsyncTaskMethodBuilder_1_tA28B0962049DC249BEC0054E2394B1962E28E90E* __this, Exception_t* ___0_exception, const RuntimeMethod* method)
{
	((  void (*) (AsyncTaskMethodBuilder_1_tA28B0962049DC249BEC0054E2394B1962E28E90E*, Exception_t*, const RuntimeMethod*))AsyncTaskMethodBuilder_1_SetException_mC2F74B26F5303F9F960965220E2866D777F1A5C6_gshared)(__this, ___0_exception, method);
}
inline void AsyncTaskMethodBuilder_1_SetResult_mB79121C0ACAC704500FAA7350ED0FE6604DDA05E (AsyncTaskMethodBuilder_1_tA28B0962049DC249BEC0054E2394B1962E28E90E* __this, String_t* ___0_result, const RuntimeMethod* method)
{
	((  void (*) (AsyncTaskMethodBuilder_1_tA28B0962049DC249BEC0054E2394B1962E28E90E*, String_t*, const RuntimeMethod*))AsyncTaskMethodBuilder_1_SetResult_m0D83195F995F9825D7A6DCDC3835D6917C43B5A6_gshared)(__this, ___0_result, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void U3CReadResponseHeaderLineAsyncU3Ed__32_MoveNext_mD870473B7A6074F6D5C10B60427DC880DBF6FFCE (U3CReadResponseHeaderLineAsyncU3Ed__32_tEEAA4B9F7D1F99D0F6B270421640B301B419757B* __this, const RuntimeMethod* method) ;
inline void AsyncTaskMethodBuilder_1_SetStateMachine_mE4D48B78E50CA2A3650A856ACF86A3CFBA249C30 (AsyncTaskMethodBuilder_1_tA28B0962049DC249BEC0054E2394B1962E28E90E* __this, RuntimeObject* ___0_stateMachine, const RuntimeMethod* method)
{
	((  void (*) (AsyncTaskMethodBuilder_1_tA28B0962049DC249BEC0054E2394B1962E28E90E*, RuntimeObject*, const RuntimeMethod*))AsyncTaskMethodBuilder_1_SetStateMachine_m3BE54983634ABF5BE05200C7894AD0F9F20BDD65_gshared)(__this, ___0_stateMachine, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void U3CReadResponseHeaderLineAsyncU3Ed__32_SetStateMachine_m20CB2EBCC14A564718D4D4F62D66B1E28CC21BDC (U3CReadResponseHeaderLineAsyncU3Ed__32_tEEAA4B9F7D1F99D0F6B270421640B301B419757B* __this, RuntimeObject* ___0_stateMachine, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR TimeSpan_t8195C5B013A2C532FEBDF0B64B6911982E750F5A TimeSpan_FromSeconds_m4401B50C0CA831134068595C6AF2F1CA92AAF80F (double ___0_value, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* __this, String_t* ___0_paramName, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ArgumentException__ctor_m8F9D40CE19D19B698A70F9A258640EB52DB39B62 (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* __this, String_t* ___0_message, String_t* ___1_paramName, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WebSocketValidate_ValidateSubprotocol_m0B17C743ACDA3DDEBC3FF44386E2863CE029EC7E (String_t* ___0_subProtocol, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool TimeSpan_op_Inequality_m2248419A8BCC8744CADE25174238B24AE34F17DB (TimeSpan_t8195C5B013A2C532FEBDF0B64B6911982E750F5A ___0_t1, TimeSpan_t8195C5B013A2C532FEBDF0B64B6911982E750F5A ___1_t2, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool TimeSpan_op_LessThan_m91C76FBEB38D80680A92A5FACA3A93810349B0FF (TimeSpan_t8195C5B013A2C532FEBDF0B64B6911982E750F5A ___0_t1, TimeSpan_t8195C5B013A2C532FEBDF0B64B6911982E750F5A ___1_t2, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* SR_Format_m9E8DC9AEFDC34AC67473EFAEAB78C5066C1A0D09 (String_t* ___0_resourceFormat, RuntimeObject* ___1_p1, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ArgumentOutOfRangeException__ctor_m60B543A63AC8692C28096003FBF2AD124B9D5B85 (ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F* __this, String_t* ___0_paramName, RuntimeObject* ___1_actualValue, String_t* ___2_message, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ManagedWebSocket_tFE006AE2E3A7EFDD703AAA7AC6FC035F65695F58* ManagedWebSocket_CreateFromConnectedStream_mD63D8BA7265EA6EAFD6F59C308B3D39EF5BFFC4C (Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* ___0_stream, bool ___1_isServer, String_t* ___2_subprotocol, TimeSpan_t8195C5B013A2C532FEBDF0B64B6911982E750F5A ___3_keepAliveInterval, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2 (RuntimeObject* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Marshal_GetLastWin32Error_mCD7EBA7503BF36F46DB58D98E96101FB67684379 (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WebSocketException__ctor_m9DE018FA255F69B31247D626CD46E906F81A2D01 (WebSocketException_t7ED58EA50E372ED89E1469A44379C2FB097A6263* __this, int32_t ___0_nativeError, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* WebSocketException_GetErrorMessage_m94EAC32BD12C668BBF74CF3189AB22916F8BB434 (int32_t ___0_error, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WebSocketException__ctor_m7350F0E619B737BD230B8653A819B04394502058 (WebSocketException_t7ED58EA50E372ED89E1469A44379C2FB097A6263* __this, int32_t ___0_error, String_t* ___1_message, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Win32Exception__ctor_m1DC405E68F70450557C7D3919BF43BC4EC137EBA (Win32Exception_t15A75629914EB77C816D8219D93ED78E50C74BE9* __this, String_t* ___0_message, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WebSocketException__ctor_m115118818FBC3CEEAB5722398FB1F4051F799A6B (WebSocketException_t7ED58EA50E372ED89E1469A44379C2FB097A6263* __this, int32_t ___0_error, String_t* ___1_message, Exception_t* ___2_innerException, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Win32Exception__ctor_m045D7B9B13CD3EF9924DA35FB6015D245484DDFF (Win32Exception_t15A75629914EB77C816D8219D93ED78E50C74BE9* __this, String_t* ___0_message, Exception_t* ___1_innerException, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Win32Exception__ctor_mFC38F1C99031D227C6BB3CA07099866D39994281 (Win32Exception_t15A75629914EB77C816D8219D93ED78E50C74BE9* __this, int32_t ___0_error, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool WebSocketException_Succeeded_mD4CDEB2018F54CD1840820718516FD425EE6ED70 (int32_t ___0_hr, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WebSocketException_SetErrorCodeOnError_m886DDADA4F582383259E13847E5E85323917ACBA (WebSocketException_t7ED58EA50E372ED89E1469A44379C2FB097A6263* __this, int32_t ___0_nativeError, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Win32Exception__ctor_m1B3DF3F51BBF050C18F933B36027A8A4902668C4 (Win32Exception_t15A75629914EB77C816D8219D93ED78E50C74BE9* __this, SerializationInfo_t3C47F63E24BEB9FCE2DC6309E027F238DC5C5E37* ___0_info, StreamingContext_t56760522A751890146EE45F82F866B55B7E33677 ___1_context, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Win32Exception_GetObjectData_mB7993FD0BE12222D5518373EBAB84F89651D99D2 (Win32Exception_t15A75629914EB77C816D8219D93ED78E50C74BE9* __this, SerializationInfo_t3C47F63E24BEB9FCE2DC6309E027F238DC5C5E37* ___0_info, StreamingContext_t56760522A751890146EE45F82F866B55B7E33677 ___1_context, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SerializationInfo_AddValue_m28FE9B110F21DDB8FF5F5E35A0EABD659DB22C2F (SerializationInfo_t3C47F63E24BEB9FCE2DC6309E027F238DC5C5E37* __this, String_t* ___0_name, RuntimeObject* ___1_value, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* SR_Format_m27BC634145CE1B8E25594A82CDBBF04AD501CA02 (String_t* ___0_resourceFormat, RuntimeObject* ___1_p1, RuntimeObject* ___2_p2, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Exception_set_HResult_m010A171183E240EBF4F7611E924B533FAB2E471F_inline (Exception_t* __this, int32_t ___0_value, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WebSocketReceiveResult__ctor_m3B47B8C4F0CC1C96FF4B314BAAB9E67E1967E186 (WebSocketReceiveResult_t31FCE9F2E53843C065CD57BE10581EE47D6FAD2C* __this, int32_t ___0_count, int32_t ___1_messageType, bool ___2_endOfMessage, Nullable_1_t4936015D54B4E5C3191E452324F5F52BD55E1284 ___3_closeStatus, String_t* ___4_closeStatusDescription, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ArgumentOutOfRangeException__ctor_mBC1D5DEEA1BA41DE77228CB27D6BAFEB6DCCBF4A (ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F* __this, String_t* ___0_paramName, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stream__ctor_mE8B074A0EBEB026FFF14062AB4B8A78E17EFFBF0 (Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* SR_GetString_m2F0EFCFA5FE3DB6DE89C5823284302D691355AA6 (String_t* ___0_name, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stream_Dispose_m9B37BD21A57F8F2BD20EE353DE14405700810C5C (Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* __this, bool ___0_disposing, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SslStream_CheckDisposed_m884E18F119B366EE6734CCFA126A1F633687FA95 (SslStream_t19A079881850F9CAD7BAA6FB625BBC4647ED5A27* __this, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR String_t* MobileAuthenticatedStream_get_TargetHost_m572DA701CB3CE3FA4006D5745F9A3B9A52739CAB_inline (MobileAuthenticatedStream_tD0306DC2B0CDA3C7DB261C19FFA35CA8EE24309E* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR MonoTlsProvider_t39C898CDC9458EEAD7C019B4B23701EAF9E24F7E* MonoTlsProviderFactory_GetProvider_m8684E3A1AFB043FA00DEC4BCF95F8B288C136936 (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SslStream__ctor_m91391419C92963CB7B2D1F1DAB448843CD07681B (SslStream_t19A079881850F9CAD7BAA6FB625BBC4647ED5A27* __this, Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* ___0_innerStream, bool ___1_leaveInnerStreamOpen, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AuthenticatedStream__ctor_m8B163146A01B938A3E87E1BED07EE07494A3E357 (AuthenticatedStream_t8DCF41E151F705E2494FC7836F5E2EF7C539FA39* __this, Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* ___0_innerStream, bool ___1_leaveInnerStreamOpen, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR MobileTlsProvider_tD60D82BEBF267F50F388A026DBB092C7188BB017* SslStream_GetProvider_mE1D070C13DB759156659F57AB9DF4D2E397FD85B (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR MonoTlsSettings_tD79AF4AE5C2CD533A3D7A08FED479B1EC1A031B0* MonoTlsSettings_CopyDefaultSettings_m4B0A3E8B7D106FA7F0D243FB2A0A4B115CD21942 (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR MobileAuthenticatedStream_tD0306DC2B0CDA3C7DB261C19FFA35CA8EE24309E* SslStream_get_Impl_m1356F55D556B6CBE922FB053F79BC930FCADF057 (SslStream_t19A079881850F9CAD7BAA6FB625BBC4647ED5A27* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Task_t751C4CC3ECD055BABA8A0B6A5DFBB4283DCA8572* MobileAuthenticatedStream_AuthenticateAsClientAsync_mBD8A0E7FFAFDE608063E545EF057304AF2C35EF2 (MobileAuthenticatedStream_tD0306DC2B0CDA3C7DB261C19FFA35CA8EE24309E* __this, String_t* ___0_targetHost, X509CertificateCollection_t2900D71D188EDCA7DEB5077D36103EE5DA6805CE* ___1_clientCertificates, int32_t ___2_enabledSslProtocols, bool ___3_checkCertificateRevocation, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void NotSupportedException__ctor_mE174750CF0247BBB47544FFD71D66BB89630945B (NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A* __this, String_t* ___0_message, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* AuthenticatedStream_get_InnerStream_m5BDA4857898A90C696DAD90FD61C536FC3EB10C7_inline (AuthenticatedStream_t8DCF41E151F705E2494FC7836F5E2EF7C539FA39* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ObjectDisposedException__ctor_mB2C8582279AF3F0C1CF9AA52DA7331BF848DFD48 (ObjectDisposedException_tC5FB29E8E980E2010A2F6A5B9B791089419F89EB* __this, String_t* ___0_objectName, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AuthenticatedStream_Dispose_m031F600EE876A06427FB0713D992FF02B9C3320F (AuthenticatedStream_t8DCF41E151F705E2494FC7836F5E2EF7C539FA39* __this, bool ___0_disposing, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Stream_Dispose_mCDB42F32A17541CCA6D3A5906827A401570B07A8 (Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Task_1_t4C228DE57804012969575431CFF12D57C875552D* Stream_ReadAsync_m734E5C146A1217C9E8FEC56ABDBD2AC33F5F8F87 (Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* __this, ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* ___0_buffer, int32_t ___1_offset, int32_t ___2_count, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* TaskToApm_Begin_mA26D2A4F6EE0582B181AFD83B913916909D02777 (Task_t751C4CC3ECD055BABA8A0B6A5DFBB4283DCA8572* ___0_task, AsyncCallback_t7FEF460CBDCFB9C5FA2EF776984778B9A4145F4C* ___1_callback, RuntimeObject* ___2_state, const RuntimeMethod* method) ;
inline int32_t TaskToApm_End_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mCA309E122256972A27C74FC814F47AD13AF623ED (RuntimeObject* ___0_asyncResult, const RuntimeMethod* method)
{
	return ((  int32_t (*) (RuntimeObject*, const RuntimeMethod*))TaskToApm_End_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mCA309E122256972A27C74FC814F47AD13AF623ED_gshared)(___0_asyncResult, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Task_t751C4CC3ECD055BABA8A0B6A5DFBB4283DCA8572* Stream_WriteAsync_m51D91C94481BB32FE1A876A789C9705F433B133D (Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* __this, ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* ___0_buffer, int32_t ___1_offset, int32_t ___2_count, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void TaskToApm_End_mDFB29EEE501409834D464F4C249A535723B7E6ED (RuntimeObject* ___0_asyncResult, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppChar String_get_Chars_mC49DF0CD2D3BE7BE97B3AD9C995BE3094F8E36D3 (String_t* __this, int32_t ___0_index, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t String_get_Length_m42625D67623FA5CC7A44D47425CE86FB946542D2_inline (String_t* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void PlatformNotSupportedException__ctor_mD5DBE8E9A6FF4B75EF02671029C6D67A51EAFBD1 (PlatformNotSupportedException_tD2BD7EB9278518AA5FE8AE75AD5D0D4298A4631A* __this, const RuntimeMethod* method) ;
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void U3CReadResponseHeaderLineAsyncU3Ed__32_MoveNext_mD870473B7A6074F6D5C10B60427DC880DBF6FFCE (U3CReadResponseHeaderLineAsyncU3Ed__32_tEEAA4B9F7D1F99D0F6B270421640B301B419757B* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AsyncTaskMethodBuilder_1_AwaitUnsafeOnCompleted_TisConfiguredTaskAwaiter_t8FC486401D1951F2426478F2050EE290197B6E80_TisU3CReadResponseHeaderLineAsyncU3Ed__32_tEEAA4B9F7D1F99D0F6B270421640B301B419757B_m43E8610E0472D376CD75485C575F5C9C8D91AE36_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AsyncTaskMethodBuilder_1_SetResult_mB79121C0ACAC704500FAA7350ED0FE6604DDA05E_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AsyncTaskMethodBuilder_1_tA28B0962049DC249BEC0054E2394B1962E28E90E_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ConfiguredTaskAwaitable_1_GetAwaiter_m7A77B1981FEC19CC7E1570EDC3F16AC5C14B4439_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ConfiguredTaskAwaiter_GetResult_mF1FF6CBD66A3F581D413793BA8C2AF58B707D9CC_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ConfiguredTaskAwaiter_get_IsCompleted_mD70263ED42C8E379EE20DBC2F218C3E629B4B4D3_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&StringBuilder_t_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Task_1_ConfigureAwait_m8203F4D13209C12845066A383E5B850D0486B209_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&WebSocketHandle_t9C8D33BD3D8553B1FD363A68ABDF5B81FC089F50_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	int32_t V_0 = 0;
	String_t* V_1 = NULL;
	Il2CppChar V_2 = 0x0;
	ConfiguredTaskAwaiter_t8FC486401D1951F2426478F2050EE290197B6E80 V_3;
	memset((&V_3), 0, sizeof(V_3));
	ConfiguredTaskAwaitable_1_t5186C81524388C1718E9AC37792D8771A443417A V_4;
	memset((&V_4), 0, sizeof(V_4));
	Exception_t* V_5 = NULL;
	il2cpp::utils::ExceptionSupportStack<RuntimeObject*, 1> __active_exceptions;
	{
		int32_t L_0 = __this->___U3CU3E1__state;
		V_0 = L_0;
	}
	try
	{
		{
			int32_t L_1 = V_0;
			if (!L_1)
			{
				goto IL_0043_1;
			}
		}
		{
			il2cpp_codegen_runtime_class_init_inline(WebSocketHandle_t9C8D33BD3D8553B1FD363A68ABDF5B81FC089F50_il2cpp_TypeInfo_var);
			StringBuilder_t* L_2 = ((WebSocketHandle_t9C8D33BD3D8553B1FD363A68ABDF5B81FC089F50_ThreadStaticFields*)il2cpp_codegen_get_thread_static_data(WebSocketHandle_t9C8D33BD3D8553B1FD363A68ABDF5B81FC089F50_il2cpp_TypeInfo_var))->___t_cachedStringBuilder;
			__this->___U3CsbU3E5__2 = L_2;
			Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CsbU3E5__2), (void*)L_2);
			StringBuilder_t* L_3 = __this->___U3CsbU3E5__2;
			if (!L_3)
			{
				goto IL_0025_1;
			}
		}
		{
			il2cpp_codegen_runtime_class_init_inline(WebSocketHandle_t9C8D33BD3D8553B1FD363A68ABDF5B81FC089F50_il2cpp_TypeInfo_var);
			((WebSocketHandle_t9C8D33BD3D8553B1FD363A68ABDF5B81FC089F50_ThreadStaticFields*)il2cpp_codegen_get_thread_static_data(WebSocketHandle_t9C8D33BD3D8553B1FD363A68ABDF5B81FC089F50_il2cpp_TypeInfo_var))->___t_cachedStringBuilder = (StringBuilder_t*)NULL;
			Il2CppCodeGenWriteBarrier((void**)(&((WebSocketHandle_t9C8D33BD3D8553B1FD363A68ABDF5B81FC089F50_ThreadStaticFields*)il2cpp_codegen_get_thread_static_data(WebSocketHandle_t9C8D33BD3D8553B1FD363A68ABDF5B81FC089F50_il2cpp_TypeInfo_var))->___t_cachedStringBuilder), (void*)(StringBuilder_t*)NULL);
			goto IL_0030_1;
		}

IL_0025_1:
		{
			StringBuilder_t* L_4 = (StringBuilder_t*)il2cpp_codegen_object_new(StringBuilder_t_il2cpp_TypeInfo_var);
			StringBuilder__ctor_m1D99713357DE05DAFA296633639DB55F8C30587D(L_4, NULL);
			__this->___U3CsbU3E5__2 = L_4;
			Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CsbU3E5__2), (void*)L_4);
		}

IL_0030_1:
		{
			ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* L_5 = (ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031*)(ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031*)SZArrayNew(ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031_il2cpp_TypeInfo_var, (uint32_t)1);
			__this->___U3CarrU3E5__3 = L_5;
			Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CarrU3E5__3), (void*)L_5);
			__this->___U3CprevCharU3E5__4 = 0;
		}

IL_0043_1:
		{
		}
		{
			auto __finallyBlock = il2cpp::utils::Finally([&]
			{

FINALLY_0149_1:
				{
					{
						int32_t L_6 = V_0;
						if ((((int32_t)L_6) >= ((int32_t)0)))
						{
							goto IL_0164_1;
						}
					}
					{
						StringBuilder_t* L_7 = __this->___U3CsbU3E5__2;
						NullCheck(L_7);
						StringBuilder_t* L_8;
						L_8 = StringBuilder_Clear_m2D76F6533574F40A4E3E2DC4E730277CBD0AF8F6(L_7, NULL);
						StringBuilder_t* L_9 = __this->___U3CsbU3E5__2;
						il2cpp_codegen_runtime_class_init_inline(WebSocketHandle_t9C8D33BD3D8553B1FD363A68ABDF5B81FC089F50_il2cpp_TypeInfo_var);
						((WebSocketHandle_t9C8D33BD3D8553B1FD363A68ABDF5B81FC089F50_ThreadStaticFields*)il2cpp_codegen_get_thread_static_data(WebSocketHandle_t9C8D33BD3D8553B1FD363A68ABDF5B81FC089F50_il2cpp_TypeInfo_var))->___t_cachedStringBuilder = L_9;
						Il2CppCodeGenWriteBarrier((void**)(&((WebSocketHandle_t9C8D33BD3D8553B1FD363A68ABDF5B81FC089F50_ThreadStaticFields*)il2cpp_codegen_get_thread_static_data(WebSocketHandle_t9C8D33BD3D8553B1FD363A68ABDF5B81FC089F50_il2cpp_TypeInfo_var))->___t_cachedStringBuilder), (void*)L_9);
					}

IL_0164_1:
					{
						return;
					}
				}
			});
			try
			{
				{
					int32_t L_10 = V_0;
					if (!L_10)
					{
						goto IL_00d0_2;
					}
				}
				{
					goto IL_007b_2;
				}

IL_004c_2:
				{
					ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* L_11 = __this->___U3CarrU3E5__3;
					NullCheck(L_11);
					int32_t L_12 = 0;
					uint8_t L_13 = (L_11)->GetAt(static_cast<il2cpp_array_size_t>(L_12));
					V_2 = L_13;
					Il2CppChar L_14 = __this->___U3CprevCharU3E5__4;
					if ((!(((uint32_t)L_14) == ((uint32_t)((int32_t)13)))))
					{
						goto IL_0067_2;
					}
				}
				{
					Il2CppChar L_15 = V_2;
					if ((((int32_t)L_15) == ((int32_t)((int32_t)10))))
					{
						goto IL_00f9_2;
					}
				}

IL_0067_2:
				{
					StringBuilder_t* L_16 = __this->___U3CsbU3E5__2;
					Il2CppChar L_17 = V_2;
					NullCheck(L_16);
					StringBuilder_t* L_18;
					L_18 = StringBuilder_Append_m71228B30F05724CD2CD96D9611DCD61BFB96A6E1(L_16, L_17, NULL);
					Il2CppChar L_19 = V_2;
					__this->___U3CprevCharU3E5__4 = L_19;
				}

IL_007b_2:
				{
					Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* L_20 = __this->___stream;
					ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* L_21 = __this->___U3CarrU3E5__3;
					CancellationToken_t51142D9C6D7C02D314DA34A6A7988C528992FFED L_22 = __this->___cancellationToken;
					NullCheck(L_20);
					Task_1_t4C228DE57804012969575431CFF12D57C875552D* L_23;
					L_23 = VirtualFuncInvoker4< Task_1_t4C228DE57804012969575431CFF12D57C875552D*, ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031*, int32_t, int32_t, CancellationToken_t51142D9C6D7C02D314DA34A6A7988C528992FFED >::Invoke(18, L_20, L_21, 0, 1, L_22);
					NullCheck(L_23);
					ConfiguredTaskAwaitable_1_t5186C81524388C1718E9AC37792D8771A443417A L_24;
					L_24 = Task_1_ConfigureAwait_m8203F4D13209C12845066A383E5B850D0486B209(L_23, (bool)0, Task_1_ConfigureAwait_m8203F4D13209C12845066A383E5B850D0486B209_RuntimeMethod_var);
					V_4 = L_24;
					ConfiguredTaskAwaiter_t8FC486401D1951F2426478F2050EE290197B6E80 L_25;
					L_25 = ConfiguredTaskAwaitable_1_GetAwaiter_m7A77B1981FEC19CC7E1570EDC3F16AC5C14B4439_inline((&V_4), ConfiguredTaskAwaitable_1_GetAwaiter_m7A77B1981FEC19CC7E1570EDC3F16AC5C14B4439_RuntimeMethod_var);
					V_3 = L_25;
					bool L_26;
					L_26 = ConfiguredTaskAwaiter_get_IsCompleted_mD70263ED42C8E379EE20DBC2F218C3E629B4B4D3((&V_3), ConfiguredTaskAwaiter_get_IsCompleted_mD70263ED42C8E379EE20DBC2F218C3E629B4B4D3_RuntimeMethod_var);
					if (L_26)
					{
						goto IL_00ec_2;
					}
				}
				{
					int32_t L_27 = 0;
					V_0 = L_27;
					__this->___U3CU3E1__state = L_27;
					ConfiguredTaskAwaiter_t8FC486401D1951F2426478F2050EE290197B6E80 L_28 = V_3;
					__this->___U3CU3Eu__1 = L_28;
					Il2CppCodeGenWriteBarrier((void**)&(((&__this->___U3CU3Eu__1))->___m_task), (void*)NULL);
					AsyncTaskMethodBuilder_1_tA28B0962049DC249BEC0054E2394B1962E28E90E* L_29 = (AsyncTaskMethodBuilder_1_tA28B0962049DC249BEC0054E2394B1962E28E90E*)(&__this->___U3CU3Et__builder);
					il2cpp_codegen_runtime_class_init_inline(AsyncTaskMethodBuilder_1_tA28B0962049DC249BEC0054E2394B1962E28E90E_il2cpp_TypeInfo_var);
					AsyncTaskMethodBuilder_1_AwaitUnsafeOnCompleted_TisConfiguredTaskAwaiter_t8FC486401D1951F2426478F2050EE290197B6E80_TisU3CReadResponseHeaderLineAsyncU3Ed__32_tEEAA4B9F7D1F99D0F6B270421640B301B419757B_m43E8610E0472D376CD75485C575F5C9C8D91AE36(L_29, (&V_3), __this, AsyncTaskMethodBuilder_1_AwaitUnsafeOnCompleted_TisConfiguredTaskAwaiter_t8FC486401D1951F2426478F2050EE290197B6E80_TisU3CReadResponseHeaderLineAsyncU3Ed__32_tEEAA4B9F7D1F99D0F6B270421640B301B419757B_m43E8610E0472D376CD75485C575F5C9C8D91AE36_RuntimeMethod_var);
					goto IL_01ae;
				}

IL_00d0_2:
				{
					ConfiguredTaskAwaiter_t8FC486401D1951F2426478F2050EE290197B6E80 L_30 = __this->___U3CU3Eu__1;
					V_3 = L_30;
					ConfiguredTaskAwaiter_t8FC486401D1951F2426478F2050EE290197B6E80* L_31 = (ConfiguredTaskAwaiter_t8FC486401D1951F2426478F2050EE290197B6E80*)(&__this->___U3CU3Eu__1);
					il2cpp_codegen_initobj(L_31, sizeof(ConfiguredTaskAwaiter_t8FC486401D1951F2426478F2050EE290197B6E80));
					int32_t L_32 = (-1);
					V_0 = L_32;
					__this->___U3CU3E1__state = L_32;
				}

IL_00ec_2:
				{
					int32_t L_33;
					L_33 = ConfiguredTaskAwaiter_GetResult_mF1FF6CBD66A3F581D413793BA8C2AF58B707D9CC((&V_3), ConfiguredTaskAwaiter_GetResult_mF1FF6CBD66A3F581D413793BA8C2AF58B707D9CC_RuntimeMethod_var);
					if ((((int32_t)L_33) == ((int32_t)1)))
					{
						goto IL_004c_2;
					}
				}

IL_00f9_2:
				{
					StringBuilder_t* L_34 = __this->___U3CsbU3E5__2;
					NullCheck(L_34);
					int32_t L_35;
					L_35 = StringBuilder_get_Length_mDEA041E7357C68CC3B5885276BB403676DAAE0D8(L_34, NULL);
					if ((((int32_t)L_35) <= ((int32_t)0)))
					{
						goto IL_013b_2;
					}
				}
				{
					StringBuilder_t* L_36 = __this->___U3CsbU3E5__2;
					StringBuilder_t* L_37 = __this->___U3CsbU3E5__2;
					NullCheck(L_37);
					int32_t L_38;
					L_38 = StringBuilder_get_Length_mDEA041E7357C68CC3B5885276BB403676DAAE0D8(L_37, NULL);
					NullCheck(L_36);
					Il2CppChar L_39;
					L_39 = StringBuilder_get_Chars_m254FD6F2F75C00B0D353D73B2A4A19316BD7624D(L_36, ((int32_t)il2cpp_codegen_subtract(L_38, 1)), NULL);
					if ((!(((uint32_t)L_39) == ((uint32_t)((int32_t)13)))))
					{
						goto IL_013b_2;
					}
				}
				{
					StringBuilder_t* L_40 = __this->___U3CsbU3E5__2;
					StringBuilder_t* L_41 = __this->___U3CsbU3E5__2;
					NullCheck(L_41);
					int32_t L_42;
					L_42 = StringBuilder_get_Length_mDEA041E7357C68CC3B5885276BB403676DAAE0D8(L_41, NULL);
					NullCheck(L_40);
					StringBuilder_set_Length_mE2427BDAEF91C4E4A6C80F3BDF1F6E01DBCC2414(L_40, ((int32_t)il2cpp_codegen_subtract(L_42, 1)), NULL);
				}

IL_013b_2:
				{
					StringBuilder_t* L_43 = __this->___U3CsbU3E5__2;
					NullCheck(L_43);
					String_t* L_44;
					L_44 = VirtualFuncInvoker0< String_t* >::Invoke(3, L_43);
					V_1 = L_44;
					goto IL_018c;
				}
			}
			catch(Il2CppExceptionWrapper& e)
			{
				__finallyBlock.StoreException(e.ex);
			}
		}
	}
	catch(Il2CppExceptionWrapper& e)
	{
		if(il2cpp_codegen_class_is_assignable_from (((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Exception_t_il2cpp_TypeInfo_var)), il2cpp_codegen_object_class(e.ex)))
		{
			IL2CPP_PUSH_ACTIVE_EXCEPTION(e.ex);
			goto CATCH_0165;
		}
		throw e;
	}

CATCH_0165:
	{
		Exception_t* L_45 = ((Exception_t*)IL2CPP_GET_ACTIVE_EXCEPTION(Exception_t*));;
		V_5 = L_45;
		__this->___U3CU3E1__state = ((int32_t)-2);
		__this->___U3CsbU3E5__2 = (StringBuilder_t*)NULL;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CsbU3E5__2), (void*)(StringBuilder_t*)NULL);
		__this->___U3CarrU3E5__3 = (ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031*)NULL;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CarrU3E5__3), (void*)(ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031*)NULL);
		AsyncTaskMethodBuilder_1_tA28B0962049DC249BEC0054E2394B1962E28E90E* L_46 = (AsyncTaskMethodBuilder_1_tA28B0962049DC249BEC0054E2394B1962E28E90E*)(&__this->___U3CU3Et__builder);
		Exception_t* L_47 = V_5;
		il2cpp_codegen_runtime_class_init_inline(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&AsyncTaskMethodBuilder_1_tA28B0962049DC249BEC0054E2394B1962E28E90E_il2cpp_TypeInfo_var)));
		AsyncTaskMethodBuilder_1_SetException_m897218C2DB8D7BA446EB263573085805B8F0638C(L_46, L_47, ((RuntimeMethod*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&AsyncTaskMethodBuilder_1_SetException_m897218C2DB8D7BA446EB263573085805B8F0638C_RuntimeMethod_var)));
		IL2CPP_POP_ACTIVE_EXCEPTION(Exception_t*);
		goto IL_01ae;
	}

IL_018c:
	{
		__this->___U3CU3E1__state = ((int32_t)-2);
		__this->___U3CsbU3E5__2 = (StringBuilder_t*)NULL;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CsbU3E5__2), (void*)(StringBuilder_t*)NULL);
		__this->___U3CarrU3E5__3 = (ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031*)NULL;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CarrU3E5__3), (void*)(ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031*)NULL);
		AsyncTaskMethodBuilder_1_tA28B0962049DC249BEC0054E2394B1962E28E90E* L_48 = (AsyncTaskMethodBuilder_1_tA28B0962049DC249BEC0054E2394B1962E28E90E*)(&__this->___U3CU3Et__builder);
		String_t* L_49 = V_1;
		il2cpp_codegen_runtime_class_init_inline(AsyncTaskMethodBuilder_1_tA28B0962049DC249BEC0054E2394B1962E28E90E_il2cpp_TypeInfo_var);
		AsyncTaskMethodBuilder_1_SetResult_mB79121C0ACAC704500FAA7350ED0FE6604DDA05E(L_48, L_49, AsyncTaskMethodBuilder_1_SetResult_mB79121C0ACAC704500FAA7350ED0FE6604DDA05E_RuntimeMethod_var);
	}

IL_01ae:
	{
		return;
	}
}
IL2CPP_EXTERN_C  void U3CReadResponseHeaderLineAsyncU3Ed__32_MoveNext_mD870473B7A6074F6D5C10B60427DC880DBF6FFCE_AdjustorThunk (RuntimeObject* __this, const RuntimeMethod* method)
{
	U3CReadResponseHeaderLineAsyncU3Ed__32_tEEAA4B9F7D1F99D0F6B270421640B301B419757B* _thisAdjusted;
	int32_t _offset = 1;
	_thisAdjusted = reinterpret_cast<U3CReadResponseHeaderLineAsyncU3Ed__32_tEEAA4B9F7D1F99D0F6B270421640B301B419757B*>(__this + _offset);
	U3CReadResponseHeaderLineAsyncU3Ed__32_MoveNext_mD870473B7A6074F6D5C10B60427DC880DBF6FFCE(_thisAdjusted, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void U3CReadResponseHeaderLineAsyncU3Ed__32_SetStateMachine_m20CB2EBCC14A564718D4D4F62D66B1E28CC21BDC (U3CReadResponseHeaderLineAsyncU3Ed__32_tEEAA4B9F7D1F99D0F6B270421640B301B419757B* __this, RuntimeObject* ___0_stateMachine, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AsyncTaskMethodBuilder_1_SetStateMachine_mE4D48B78E50CA2A3650A856ACF86A3CFBA249C30_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AsyncTaskMethodBuilder_1_tA28B0962049DC249BEC0054E2394B1962E28E90E_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		AsyncTaskMethodBuilder_1_tA28B0962049DC249BEC0054E2394B1962E28E90E* L_0 = (AsyncTaskMethodBuilder_1_tA28B0962049DC249BEC0054E2394B1962E28E90E*)(&__this->___U3CU3Et__builder);
		RuntimeObject* L_1 = ___0_stateMachine;
		il2cpp_codegen_runtime_class_init_inline(AsyncTaskMethodBuilder_1_tA28B0962049DC249BEC0054E2394B1962E28E90E_il2cpp_TypeInfo_var);
		AsyncTaskMethodBuilder_1_SetStateMachine_mE4D48B78E50CA2A3650A856ACF86A3CFBA249C30(L_0, L_1, AsyncTaskMethodBuilder_1_SetStateMachine_mE4D48B78E50CA2A3650A856ACF86A3CFBA249C30_RuntimeMethod_var);
		return;
	}
}
IL2CPP_EXTERN_C  void U3CReadResponseHeaderLineAsyncU3Ed__32_SetStateMachine_m20CB2EBCC14A564718D4D4F62D66B1E28CC21BDC_AdjustorThunk (RuntimeObject* __this, RuntimeObject* ___0_stateMachine, const RuntimeMethod* method)
{
	U3CReadResponseHeaderLineAsyncU3Ed__32_tEEAA4B9F7D1F99D0F6B270421640B301B419757B* _thisAdjusted;
	int32_t _offset = 1;
	_thisAdjusted = reinterpret_cast<U3CReadResponseHeaderLineAsyncU3Ed__32_tEEAA4B9F7D1F99D0F6B270421640B301B419757B*>(__this + _offset);
	U3CReadResponseHeaderLineAsyncU3Ed__32_SetStateMachine_m20CB2EBCC14A564718D4D4F62D66B1E28CC21BDC(_thisAdjusted, ___0_stateMachine, method);
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR TimeSpan_t8195C5B013A2C532FEBDF0B64B6911982E750F5A WebSocket_get_DefaultKeepAliveInterval_mEA61DD776ED578DB4D2063583CCFF522663ABEAC (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&TimeSpan_t8195C5B013A2C532FEBDF0B64B6911982E750F5A_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_runtime_class_init_inline(TimeSpan_t8195C5B013A2C532FEBDF0B64B6911982E750F5A_il2cpp_TypeInfo_var);
		TimeSpan_t8195C5B013A2C532FEBDF0B64B6911982E750F5A L_0;
		L_0 = TimeSpan_FromSeconds_m4401B50C0CA831134068595C6AF2F1CA92AAF80F((30.0), NULL);
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR WebSocket_tE3DEAFF0F68BD5AF9A526D29D56B6E5C6E24C3A1* WebSocket_CreateClientWebSocket_m5E6C94310C5ED85B1A7B772B831C832EFF76FFA7 (Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* ___0_innerStream, String_t* ___1_subProtocol, int32_t ___2_receiveBufferSize, int32_t ___3_sendBufferSize, TimeSpan_t8195C5B013A2C532FEBDF0B64B6911982E750F5A ___4_keepAliveInterval, bool ___5_useZeroMaskingKey, ArraySegment_1_t3DC888623B720A071D69279F1FCB95A109195093 ___6_internalBuffer, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ManagedWebSocket_tFE006AE2E3A7EFDD703AAA7AC6FC035F65695F58_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&TimeSpan_t8195C5B013A2C532FEBDF0B64B6911982E750F5A_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Timeout_tDBB45B56C59D1AF9A6B1EC5F1E3200D12AB18E41_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	String_t* G_B7_0 = NULL;
	String_t* G_B18_0 = NULL;
	String_t* G_B20_0 = NULL;
	String_t* G_B19_0 = NULL;
	int32_t G_B21_0 = 0;
	String_t* G_B21_1 = NULL;
	{
		Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* L_0 = ___0_innerStream;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_1 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_1, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralA6EAFAF4139980ACB770E237272125EB9180B7A6)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, ((RuntimeMethod*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&WebSocket_CreateClientWebSocket_m5E6C94310C5ED85B1A7B772B831C832EFF76FFA7_RuntimeMethod_var)));
	}

IL_000e:
	{
		Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* L_2 = ___0_innerStream;
		NullCheck(L_2);
		bool L_3;
		L_3 = VirtualFuncInvoker0< bool >::Invoke(7, L_2);
		if (!L_3)
		{
			goto IL_001e;
		}
	}
	{
		Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* L_4 = ___0_innerStream;
		NullCheck(L_4);
		bool L_5;
		L_5 = VirtualFuncInvoker0< bool >::Invoke(9, L_4);
		if (L_5)
		{
			goto IL_003d;
		}
	}

IL_001e:
	{
		Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* L_6 = ___0_innerStream;
		NullCheck(L_6);
		bool L_7;
		L_7 = VirtualFuncInvoker0< bool >::Invoke(7, L_6);
		if (!L_7)
		{
			goto IL_002d;
		}
	}
	{
		G_B7_0 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralAA0F00452EE7B525EB6EC13E514BABDE9C00576A));
		goto IL_0032;
	}

IL_002d:
	{
		G_B7_0 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral95C3DD8E6E01DC5FE6AC3E2B6375E6AD246F8241));
	}

IL_0032:
	{
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_8 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m8F9D40CE19D19B698A70F9A258640EB52DB39B62(L_8, G_B7_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralA6EAFAF4139980ACB770E237272125EB9180B7A6)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_8, ((RuntimeMethod*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&WebSocket_CreateClientWebSocket_m5E6C94310C5ED85B1A7B772B831C832EFF76FFA7_RuntimeMethod_var)));
	}

IL_003d:
	{
		String_t* L_9 = ___1_subProtocol;
		if (!L_9)
		{
			goto IL_0046;
		}
	}
	{
		String_t* L_10 = ___1_subProtocol;
		WebSocketValidate_ValidateSubprotocol_m0B17C743ACDA3DDEBC3FF44386E2863CE029EC7E(L_10, NULL);
	}

IL_0046:
	{
		TimeSpan_t8195C5B013A2C532FEBDF0B64B6911982E750F5A L_11 = ___4_keepAliveInterval;
		il2cpp_codegen_runtime_class_init_inline(Timeout_tDBB45B56C59D1AF9A6B1EC5F1E3200D12AB18E41_il2cpp_TypeInfo_var);
		TimeSpan_t8195C5B013A2C532FEBDF0B64B6911982E750F5A L_12 = ((Timeout_tDBB45B56C59D1AF9A6B1EC5F1E3200D12AB18E41_StaticFields*)il2cpp_codegen_static_fields_for(Timeout_tDBB45B56C59D1AF9A6B1EC5F1E3200D12AB18E41_il2cpp_TypeInfo_var))->___InfiniteTimeSpan;
		il2cpp_codegen_runtime_class_init_inline(TimeSpan_t8195C5B013A2C532FEBDF0B64B6911982E750F5A_il2cpp_TypeInfo_var);
		bool L_13;
		L_13 = TimeSpan_op_Inequality_m2248419A8BCC8744CADE25174238B24AE34F17DB(L_11, L_12, NULL);
		if (!L_13)
		{
			goto IL_0084;
		}
	}
	{
		TimeSpan_t8195C5B013A2C532FEBDF0B64B6911982E750F5A L_14 = ___4_keepAliveInterval;
		il2cpp_codegen_runtime_class_init_inline(TimeSpan_t8195C5B013A2C532FEBDF0B64B6911982E750F5A_il2cpp_TypeInfo_var);
		TimeSpan_t8195C5B013A2C532FEBDF0B64B6911982E750F5A L_15 = ((TimeSpan_t8195C5B013A2C532FEBDF0B64B6911982E750F5A_StaticFields*)il2cpp_codegen_static_fields_for(TimeSpan_t8195C5B013A2C532FEBDF0B64B6911982E750F5A_il2cpp_TypeInfo_var))->___Zero;
		bool L_16;
		L_16 = TimeSpan_op_LessThan_m91C76FBEB38D80680A92A5FACA3A93810349B0FF(L_14, L_15, NULL);
		if (!L_16)
		{
			goto IL_0084;
		}
	}
	{
		TimeSpan_t8195C5B013A2C532FEBDF0B64B6911982E750F5A L_17 = ___4_keepAliveInterval;
		TimeSpan_t8195C5B013A2C532FEBDF0B64B6911982E750F5A L_18 = L_17;
		RuntimeObject* L_19 = Box(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&TimeSpan_t8195C5B013A2C532FEBDF0B64B6911982E750F5A_il2cpp_TypeInfo_var)), &L_18);
		int32_t L_20 = 0;
		RuntimeObject* L_21 = Box(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var)), &L_20);
		String_t* L_22;
		L_22 = SR_Format_m9E8DC9AEFDC34AC67473EFAEAB78C5066C1A0D09(((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralA8327EDA8A27CFF2C0328F41898D104AE50B8B3B)), L_21, NULL);
		ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F* L_23 = (ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F_il2cpp_TypeInfo_var)));
		ArgumentOutOfRangeException__ctor_m60B543A63AC8692C28096003FBF2AD124B9D5B85(L_23, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralBDC5FDC47ACE7F03CD95EC2B3BB2812C5F79E14C)), L_19, L_22, NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_23, ((RuntimeMethod*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&WebSocket_CreateClientWebSocket_m5E6C94310C5ED85B1A7B772B831C832EFF76FFA7_RuntimeMethod_var)));
	}

IL_0084:
	{
		int32_t L_24 = ___2_receiveBufferSize;
		if ((((int32_t)L_24) <= ((int32_t)0)))
		{
			goto IL_008c;
		}
	}
	{
		int32_t L_25 = ___3_sendBufferSize;
		if ((((int32_t)L_25) > ((int32_t)0)))
		{
			goto IL_00bf;
		}
	}

IL_008c:
	{
		int32_t L_26 = ___2_receiveBufferSize;
		if ((((int32_t)L_26) <= ((int32_t)0)))
		{
			goto IL_0097;
		}
	}
	{
		G_B18_0 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral5246246403494F3045678A959CC58FB0501C504D));
		goto IL_009c;
	}

IL_0097:
	{
		G_B18_0 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral8CE29B903D391961B0EC3D0BC630ABAF3D763A92));
	}

IL_009c:
	{
		int32_t L_27 = ___2_receiveBufferSize;
		if ((((int32_t)L_27) <= ((int32_t)0)))
		{
			G_B20_0 = G_B18_0;
			goto IL_00a3;
		}
		G_B19_0 = G_B18_0;
	}
	{
		int32_t L_28 = ___3_sendBufferSize;
		G_B21_0 = L_28;
		G_B21_1 = G_B19_0;
		goto IL_00a4;
	}

IL_00a3:
	{
		int32_t L_29 = ___2_receiveBufferSize;
		G_B21_0 = L_29;
		G_B21_1 = G_B20_0;
	}

IL_00a4:
	{
		int32_t L_30 = G_B21_0;
		RuntimeObject* L_31 = Box(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var)), &L_30);
		int32_t L_32 = 0;
		RuntimeObject* L_33 = Box(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_il2cpp_TypeInfo_var)), &L_32);
		String_t* L_34;
		L_34 = SR_Format_m9E8DC9AEFDC34AC67473EFAEAB78C5066C1A0D09(((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralA8327EDA8A27CFF2C0328F41898D104AE50B8B3B)), L_33, NULL);
		ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F* L_35 = (ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F_il2cpp_TypeInfo_var)));
		ArgumentOutOfRangeException__ctor_m60B543A63AC8692C28096003FBF2AD124B9D5B85(L_35, G_B21_1, L_31, L_34, NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_35, ((RuntimeMethod*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&WebSocket_CreateClientWebSocket_m5E6C94310C5ED85B1A7B772B831C832EFF76FFA7_RuntimeMethod_var)));
	}

IL_00bf:
	{
		Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* L_36 = ___0_innerStream;
		String_t* L_37 = ___1_subProtocol;
		TimeSpan_t8195C5B013A2C532FEBDF0B64B6911982E750F5A L_38 = ___4_keepAliveInterval;
		il2cpp_codegen_runtime_class_init_inline(ManagedWebSocket_tFE006AE2E3A7EFDD703AAA7AC6FC035F65695F58_il2cpp_TypeInfo_var);
		ManagedWebSocket_tFE006AE2E3A7EFDD703AAA7AC6FC035F65695F58* L_39;
		L_39 = ManagedWebSocket_CreateFromConnectedStream_mD63D8BA7265EA6EAFD6F59C308B3D39EF5BFFC4C(L_36, (bool)0, L_37, L_38, NULL);
		return L_39;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WebSocket__ctor_m27B401AAA7211E0DC5358B6B7424B113CDD49B34 (WebSocket_tE3DEAFF0F68BD5AF9A526D29D56B6E5C6E24C3A1* __this, const RuntimeMethod* method) 
{
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2(__this, NULL);
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WebSocketException__ctor_m9459919CC9E470BBFEB4803C5941475FCFD9A093 (WebSocketException_t7ED58EA50E372ED89E1469A44379C2FB097A6263* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Marshal_tD976A56A90263C3CE2B780D4B1CADADE2E70B4A7_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_runtime_class_init_inline(Marshal_tD976A56A90263C3CE2B780D4B1CADADE2E70B4A7_il2cpp_TypeInfo_var);
		int32_t L_0;
		L_0 = Marshal_GetLastWin32Error_mCD7EBA7503BF36F46DB58D98E96101FB67684379(NULL);
		WebSocketException__ctor_m9DE018FA255F69B31247D626CD46E906F81A2D01(__this, L_0, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WebSocketException__ctor_m4B3203CEBCBE0C4C7DD4D267DCFB317B3D0279BD (WebSocketException_t7ED58EA50E372ED89E1469A44379C2FB097A6263* __this, int32_t ___0_error, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = ___0_error;
		int32_t L_1 = ___0_error;
		String_t* L_2;
		L_2 = WebSocketException_GetErrorMessage_m94EAC32BD12C668BBF74CF3189AB22916F8BB434(L_1, NULL);
		WebSocketException__ctor_m7350F0E619B737BD230B8653A819B04394502058(__this, L_0, L_2, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WebSocketException__ctor_m7350F0E619B737BD230B8653A819B04394502058 (WebSocketException_t7ED58EA50E372ED89E1469A44379C2FB097A6263* __this, int32_t ___0_error, String_t* ___1_message, const RuntimeMethod* method) 
{
	{
		String_t* L_0 = ___1_message;
		Win32Exception__ctor_m1DC405E68F70450557C7D3919BF43BC4EC137EBA(__this, L_0, NULL);
		int32_t L_1 = ___0_error;
		__this->____webSocketErrorCode = L_1;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WebSocketException__ctor_mE02C70B5F472F832B548A6EBF802EBCB06EC7616 (WebSocketException_t7ED58EA50E372ED89E1469A44379C2FB097A6263* __this, int32_t ___0_error, Exception_t* ___1_innerException, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = ___0_error;
		int32_t L_1 = ___0_error;
		String_t* L_2;
		L_2 = WebSocketException_GetErrorMessage_m94EAC32BD12C668BBF74CF3189AB22916F8BB434(L_1, NULL);
		Exception_t* L_3 = ___1_innerException;
		WebSocketException__ctor_m115118818FBC3CEEAB5722398FB1F4051F799A6B(__this, L_0, L_2, L_3, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WebSocketException__ctor_m115118818FBC3CEEAB5722398FB1F4051F799A6B (WebSocketException_t7ED58EA50E372ED89E1469A44379C2FB097A6263* __this, int32_t ___0_error, String_t* ___1_message, Exception_t* ___2_innerException, const RuntimeMethod* method) 
{
	{
		String_t* L_0 = ___1_message;
		Exception_t* L_1 = ___2_innerException;
		Win32Exception__ctor_m045D7B9B13CD3EF9924DA35FB6015D245484DDFF(__this, L_0, L_1, NULL);
		int32_t L_2 = ___0_error;
		__this->____webSocketErrorCode = L_2;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WebSocketException__ctor_m9DE018FA255F69B31247D626CD46E906F81A2D01 (WebSocketException_t7ED58EA50E372ED89E1469A44379C2FB097A6263* __this, int32_t ___0_nativeError, const RuntimeMethod* method) 
{
	WebSocketException_t7ED58EA50E372ED89E1469A44379C2FB097A6263* G_B2_0 = NULL;
	WebSocketException_t7ED58EA50E372ED89E1469A44379C2FB097A6263* G_B1_0 = NULL;
	int32_t G_B3_0 = 0;
	WebSocketException_t7ED58EA50E372ED89E1469A44379C2FB097A6263* G_B3_1 = NULL;
	{
		int32_t L_0 = ___0_nativeError;
		Win32Exception__ctor_mFC38F1C99031D227C6BB3CA07099866D39994281(__this, L_0, NULL);
		int32_t L_1 = ___0_nativeError;
		bool L_2;
		L_2 = WebSocketException_Succeeded_mD4CDEB2018F54CD1840820718516FD425EE6ED70(L_1, NULL);
		if (!L_2)
		{
			G_B2_0 = __this;
			goto IL_0013;
		}
		G_B1_0 = __this;
	}
	{
		G_B3_0 = 0;
		G_B3_1 = G_B1_0;
		goto IL_0014;
	}

IL_0013:
	{
		G_B3_0 = 3;
		G_B3_1 = G_B2_0;
	}

IL_0014:
	{
		NullCheck(G_B3_1);
		G_B3_1->____webSocketErrorCode = G_B3_0;
		int32_t L_3 = ___0_nativeError;
		WebSocketException_SetErrorCodeOnError_m886DDADA4F582383259E13847E5E85323917ACBA(__this, L_3, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WebSocketException__ctor_mB26AB05064EB4ECA945759B51AB8EDA4DFAFC3E2 (WebSocketException_t7ED58EA50E372ED89E1469A44379C2FB097A6263* __this, String_t* ___0_message, const RuntimeMethod* method) 
{
	{
		String_t* L_0 = ___0_message;
		Win32Exception__ctor_m1DC405E68F70450557C7D3919BF43BC4EC137EBA(__this, L_0, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WebSocketException__ctor_m7238A6D1725B4841E9FF761282449E685F108C68 (WebSocketException_t7ED58EA50E372ED89E1469A44379C2FB097A6263* __this, String_t* ___0_message, Exception_t* ___1_innerException, const RuntimeMethod* method) 
{
	{
		String_t* L_0 = ___0_message;
		Exception_t* L_1 = ___1_innerException;
		Win32Exception__ctor_m045D7B9B13CD3EF9924DA35FB6015D245484DDFF(__this, L_0, L_1, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WebSocketException__ctor_m4F20A99C7B6D68EE3999492DE6CB15D40C5DC086 (WebSocketException_t7ED58EA50E372ED89E1469A44379C2FB097A6263* __this, SerializationInfo_t3C47F63E24BEB9FCE2DC6309E027F238DC5C5E37* ___0_serializationInfo, StreamingContext_t56760522A751890146EE45F82F866B55B7E33677 ___1_streamingContext, const RuntimeMethod* method) 
{
	{
		SerializationInfo_t3C47F63E24BEB9FCE2DC6309E027F238DC5C5E37* L_0 = ___0_serializationInfo;
		StreamingContext_t56760522A751890146EE45F82F866B55B7E33677 L_1 = ___1_streamingContext;
		Win32Exception__ctor_m1B3DF3F51BBF050C18F933B36027A8A4902668C4(__this, L_0, L_1, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WebSocketException_GetObjectData_mF818DFE58B1E953CA4361047CDDF16AFAEBFFCD6 (WebSocketException_t7ED58EA50E372ED89E1469A44379C2FB097A6263* __this, SerializationInfo_t3C47F63E24BEB9FCE2DC6309E027F238DC5C5E37* ___0_info, StreamingContext_t56760522A751890146EE45F82F866B55B7E33677 ___1_context, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&WebSocketError_tEC1353C241C6481FE77428ABEB57E39B8E6E7672_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral8A09CD330FFB1377C86A2013D5C1E2F907AE1682);
		s_Il2CppMethodInitialized = true;
	}
	{
		SerializationInfo_t3C47F63E24BEB9FCE2DC6309E027F238DC5C5E37* L_0 = ___0_info;
		StreamingContext_t56760522A751890146EE45F82F866B55B7E33677 L_1 = ___1_context;
		Win32Exception_GetObjectData_mB7993FD0BE12222D5518373EBAB84F89651D99D2(__this, L_0, L_1, NULL);
		SerializationInfo_t3C47F63E24BEB9FCE2DC6309E027F238DC5C5E37* L_2 = ___0_info;
		int32_t L_3 = __this->____webSocketErrorCode;
		int32_t L_4 = L_3;
		RuntimeObject* L_5 = Box(WebSocketError_tEC1353C241C6481FE77428ABEB57E39B8E6E7672_il2cpp_TypeInfo_var, &L_4);
		NullCheck(L_2);
		SerializationInfo_AddValue_m28FE9B110F21DDB8FF5F5E35A0EABD659DB22C2F(L_2, _stringLiteral8A09CD330FFB1377C86A2013D5C1E2F907AE1682, L_5, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* WebSocketException_GetErrorMessage_m94EAC32BD12C668BBF74CF3189AB22916F8BB434 (int32_t ___0_error, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral0095EE92A43C893ED16C93F6273DD887E60677F6);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral08F6264E77829B6ED2AD7EB9B887F08ED5AEEEE8);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral14AEEE32A8599AABC74AA8FC4206A0BF642F3F20);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral3CE4531C9B5C100D000F5224788AD0FC73FBFBB9);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral491C803A42402C98C26F444D4AFAEB35E12580D4);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral50C6B78B527F35406BA05478189F5D0C7CC514B4);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral8CAC69067702430A6F00B8758861B928C4AA88C4);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral969FB05E796DDD94836D21AF137D831A5BAE54DC);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralDA5FA4E2B91DB334E0C213C29BD90197D7DD1670);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralE1BC7174A658D0FC8737866D17DA79C9ED801F4E);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralE7640D55535D38F6521AEF64C237E62A7B71E76D);
		s_Il2CppMethodInitialized = true;
	}
	{
		int32_t L_0 = ___0_error;
		switch (((int32_t)il2cpp_codegen_subtract((int32_t)L_0, 1)))
		{
			case 0:
			{
				goto IL_002e;
			}
			case 1:
			{
				goto IL_0043;
			}
			case 2:
			{
				goto IL_006d;
			}
			case 3:
			{
				goto IL_0049;
			}
			case 4:
			{
				goto IL_004f;
			}
			case 5:
			{
				goto IL_0055;
			}
			case 6:
			{
				goto IL_005b;
			}
			case 7:
			{
				goto IL_0061;
			}
			case 8:
			{
				goto IL_0067;
			}
		}
	}
	{
		goto IL_006d;
	}

IL_002e:
	{
		String_t* L_1;
		L_1 = SR_Format_m27BC634145CE1B8E25594A82CDBBF04AD501CA02(_stringLiteral491C803A42402C98C26F444D4AFAEB35E12580D4, _stringLiteral14AEEE32A8599AABC74AA8FC4206A0BF642F3F20, _stringLiteral08F6264E77829B6ED2AD7EB9B887F08ED5AEEEE8, NULL);
		return L_1;
	}

IL_0043:
	{
		return _stringLiteralDA5FA4E2B91DB334E0C213C29BD90197D7DD1670;
	}

IL_0049:
	{
		return _stringLiteralE1BC7174A658D0FC8737866D17DA79C9ED801F4E;
	}

IL_004f:
	{
		return _stringLiteral969FB05E796DDD94836D21AF137D831A5BAE54DC;
	}

IL_0055:
	{
		return _stringLiteral8CAC69067702430A6F00B8758861B928C4AA88C4;
	}

IL_005b:
	{
		return _stringLiteralE7640D55535D38F6521AEF64C237E62A7B71E76D;
	}

IL_0061:
	{
		return _stringLiteral50C6B78B527F35406BA05478189F5D0C7CC514B4;
	}

IL_0067:
	{
		return _stringLiteral3CE4531C9B5C100D000F5224788AD0FC73FBFBB9;
	}

IL_006d:
	{
		return _stringLiteral0095EE92A43C893ED16C93F6273DD887E60677F6;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WebSocketException_SetErrorCodeOnError_m886DDADA4F582383259E13847E5E85323917ACBA (WebSocketException_t7ED58EA50E372ED89E1469A44379C2FB097A6263* __this, int32_t ___0_nativeError, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = ___0_nativeError;
		bool L_1;
		L_1 = WebSocketException_Succeeded_mD4CDEB2018F54CD1840820718516FD425EE6ED70(L_0, NULL);
		if (L_1)
		{
			goto IL_000f;
		}
	}
	{
		int32_t L_2 = ___0_nativeError;
		Exception_set_HResult_m010A171183E240EBF4F7611E924B533FAB2E471F_inline(__this, L_2, NULL);
	}

IL_000f:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool WebSocketException_Succeeded_mD4CDEB2018F54CD1840820718516FD425EE6ED70 (int32_t ___0_hr, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = ___0_hr;
		return (bool)((((int32_t)((((int32_t)L_0) < ((int32_t)0))? 1 : 0)) == ((int32_t)0))? 1 : 0);
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WebSocketReceiveResult__ctor_m67E99C6CF0702FA8BA419E86CFA14A929D3B8517 (WebSocketReceiveResult_t31FCE9F2E53843C065CD57BE10581EE47D6FAD2C* __this, int32_t ___0_count, int32_t ___1_messageType, bool ___2_endOfMessage, const RuntimeMethod* method) 
{
	Nullable_1_t4936015D54B4E5C3191E452324F5F52BD55E1284 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		int32_t L_0 = ___0_count;
		int32_t L_1 = ___1_messageType;
		bool L_2 = ___2_endOfMessage;
		il2cpp_codegen_initobj((&V_0), sizeof(Nullable_1_t4936015D54B4E5C3191E452324F5F52BD55E1284));
		Nullable_1_t4936015D54B4E5C3191E452324F5F52BD55E1284 L_3 = V_0;
		WebSocketReceiveResult__ctor_m3B47B8C4F0CC1C96FF4B314BAAB9E67E1967E186(__this, L_0, L_1, L_2, L_3, (String_t*)NULL, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WebSocketReceiveResult__ctor_m3B47B8C4F0CC1C96FF4B314BAAB9E67E1967E186 (WebSocketReceiveResult_t31FCE9F2E53843C065CD57BE10581EE47D6FAD2C* __this, int32_t ___0_count, int32_t ___1_messageType, bool ___2_endOfMessage, Nullable_1_t4936015D54B4E5C3191E452324F5F52BD55E1284 ___3_closeStatus, String_t* ___4_closeStatusDescription, const RuntimeMethod* method) 
{
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2(__this, NULL);
		int32_t L_0 = ___0_count;
		if ((((int32_t)L_0) >= ((int32_t)0)))
		{
			goto IL_0015;
		}
	}
	{
		ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F* L_1 = (ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F_il2cpp_TypeInfo_var)));
		ArgumentOutOfRangeException__ctor_mBC1D5DEEA1BA41DE77228CB27D6BAFEB6DCCBF4A(L_1, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral07624473F417C06C74D59C64840A1532FCE2C626)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, ((RuntimeMethod*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&WebSocketReceiveResult__ctor_m3B47B8C4F0CC1C96FF4B314BAAB9E67E1967E186_RuntimeMethod_var)));
	}

IL_0015:
	{
		int32_t L_2 = ___0_count;
		__this->___U3CCountU3Ek__BackingField = L_2;
		bool L_3 = ___2_endOfMessage;
		__this->___U3CEndOfMessageU3Ek__BackingField = L_3;
		int32_t L_4 = ___1_messageType;
		__this->___U3CMessageTypeU3Ek__BackingField = L_4;
		Nullable_1_t4936015D54B4E5C3191E452324F5F52BD55E1284 L_5 = ___3_closeStatus;
		__this->___U3CCloseStatusU3Ek__BackingField = L_5;
		String_t* L_6 = ___4_closeStatusDescription;
		__this->___U3CCloseStatusDescriptionU3Ek__BackingField = L_6;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CCloseStatusDescriptionU3Ek__BackingField), (void*)L_6);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t WebSocketReceiveResult_get_Count_mCD1591C6EAFAA98DDEBA26821531A80EB9E614C6 (WebSocketReceiveResult_t31FCE9F2E53843C065CD57BE10581EE47D6FAD2C* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->___U3CCountU3Ek__BackingField;
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool WebSocketReceiveResult_get_EndOfMessage_mC01D0E54A8A32BB01FC96087AA5B3ACB07EB062A (WebSocketReceiveResult_t31FCE9F2E53843C065CD57BE10581EE47D6FAD2C* __this, const RuntimeMethod* method) 
{
	{
		bool L_0 = __this->___U3CEndOfMessageU3Ek__BackingField;
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t WebSocketReceiveResult_get_MessageType_m227E3FB536FEC68E06F1070195AF20BD032EBF50 (WebSocketReceiveResult_t31FCE9F2E53843C065CD57BE10581EE47D6FAD2C* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->___U3CMessageTypeU3Ek__BackingField;
		return L_0;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool SslClientAuthenticationOptions_get_AllowRenegotiation_m3FAAF852D579A7F688961D77D611E0A096791DA9 (SslClientAuthenticationOptions_tA5277DAACAFB33887D2513D9399C5154B0D5CBC9* __this, const RuntimeMethod* method) 
{
	{
		bool L_0 = __this->____allowRenegotiation;
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* SslClientAuthenticationOptions_get_TargetHost_mC4CF1905E7B4F8193544D4997E305406B976A797 (SslClientAuthenticationOptions_tA5277DAACAFB33887D2513D9399C5154B0D5CBC9* __this, const RuntimeMethod* method) 
{
	{
		String_t* L_0 = __this->___U3CTargetHostU3Ek__BackingField;
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SslClientAuthenticationOptions_set_TargetHost_m0D8FF94F267DC51F2BDE6E1C94C2CD67783C4019 (SslClientAuthenticationOptions_tA5277DAACAFB33887D2513D9399C5154B0D5CBC9* __this, String_t* ___0_value, const RuntimeMethod* method) 
{
	{
		String_t* L_0 = ___0_value;
		__this->___U3CTargetHostU3Ek__BackingField = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CTargetHostU3Ek__BackingField), (void*)L_0);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR X509CertificateCollection_t2900D71D188EDCA7DEB5077D36103EE5DA6805CE* SslClientAuthenticationOptions_get_ClientCertificates_mD6898001726E82ADD9E3083D78BD05DB619B313C (SslClientAuthenticationOptions_tA5277DAACAFB33887D2513D9399C5154B0D5CBC9* __this, const RuntimeMethod* method) 
{
	{
		X509CertificateCollection_t2900D71D188EDCA7DEB5077D36103EE5DA6805CE* L_0 = __this->___U3CClientCertificatesU3Ek__BackingField;
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SslClientAuthenticationOptions_set_ClientCertificates_m08B053F1C158912EEA43A4A28D37A740041CA78B (SslClientAuthenticationOptions_tA5277DAACAFB33887D2513D9399C5154B0D5CBC9* __this, X509CertificateCollection_t2900D71D188EDCA7DEB5077D36103EE5DA6805CE* ___0_value, const RuntimeMethod* method) 
{
	{
		X509CertificateCollection_t2900D71D188EDCA7DEB5077D36103EE5DA6805CE* L_0 = ___0_value;
		__this->___U3CClientCertificatesU3Ek__BackingField = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CClientCertificatesU3Ek__BackingField), (void*)L_0);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SslClientAuthenticationOptions_set_CertificateRevocationCheckMode_m5DB15346690DAC5A8F610AA8C7DEC7F2DFC29DE9 (SslClientAuthenticationOptions_tA5277DAACAFB33887D2513D9399C5154B0D5CBC9* __this, int32_t ___0_value, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = ___0_value;
		if (!L_0)
		{
			goto IL_0025;
		}
	}
	{
		int32_t L_1 = ___0_value;
		if ((((int32_t)L_1) == ((int32_t)2)))
		{
			goto IL_0025;
		}
	}
	{
		int32_t L_2 = ___0_value;
		if ((((int32_t)L_2) == ((int32_t)1)))
		{
			goto IL_0025;
		}
	}
	{
		String_t* L_3;
		L_3 = SR_Format_m9E8DC9AEFDC34AC67473EFAEAB78C5066C1A0D09(((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral215B9008FEB54933AF219588889451A0CB610D34)), ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral73E7438EC9EFF139A2E1CEA81F03B87F9EF5CF0A)), NULL);
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_4 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m8F9D40CE19D19B698A70F9A258640EB52DB39B62(L_4, L_3, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral46F273EF641E07D271D91E0DC24A4392582671F8)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_4, ((RuntimeMethod*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&SslClientAuthenticationOptions_set_CertificateRevocationCheckMode_m5DB15346690DAC5A8F610AA8C7DEC7F2DFC29DE9_RuntimeMethod_var)));
	}

IL_0025:
	{
		int32_t L_5 = ___0_value;
		__this->____checkCertificateRevocation = L_5;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SslClientAuthenticationOptions_set_EncryptionPolicy_m008E6F28D3C4BFEFFB32A9BEA6AEF1C2B53F1C77 (SslClientAuthenticationOptions_tA5277DAACAFB33887D2513D9399C5154B0D5CBC9* __this, int32_t ___0_value, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = ___0_value;
		if (!L_0)
		{
			goto IL_0025;
		}
	}
	{
		int32_t L_1 = ___0_value;
		if ((((int32_t)L_1) == ((int32_t)1)))
		{
			goto IL_0025;
		}
	}
	{
		int32_t L_2 = ___0_value;
		if ((((int32_t)L_2) == ((int32_t)2)))
		{
			goto IL_0025;
		}
	}
	{
		String_t* L_3;
		L_3 = SR_Format_m9E8DC9AEFDC34AC67473EFAEAB78C5066C1A0D09(((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral215B9008FEB54933AF219588889451A0CB610D34)), ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralCB9340620E0F239BC39D9B1AA9CCF6119A0BB3FF)), NULL);
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_4 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m8F9D40CE19D19B698A70F9A258640EB52DB39B62(L_4, L_3, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral46F273EF641E07D271D91E0DC24A4392582671F8)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_4, ((RuntimeMethod*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&SslClientAuthenticationOptions_set_EncryptionPolicy_m008E6F28D3C4BFEFFB32A9BEA6AEF1C2B53F1C77_RuntimeMethod_var)));
	}

IL_0025:
	{
		int32_t L_5 = ___0_value;
		__this->____encryptionPolicy = L_5;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t SslClientAuthenticationOptions_get_EnabledSslProtocols_mEC43F31DCFB3322900792809C72AC48070A9D93B (SslClientAuthenticationOptions_tA5277DAACAFB33887D2513D9399C5154B0D5CBC9* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____enabledSslProtocols;
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SslClientAuthenticationOptions_set_EnabledSslProtocols_mC0D304F69A5DC05292B87E00A73415BC83A632D5 (SslClientAuthenticationOptions_tA5277DAACAFB33887D2513D9399C5154B0D5CBC9* __this, int32_t ___0_value, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = ___0_value;
		__this->____enabledSslProtocols = L_0;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SslClientAuthenticationOptions__ctor_mFFA8C3E39592A8376E7C3F6AC6FB89A6B7A702CD (SslClientAuthenticationOptions_tA5277DAACAFB33887D2513D9399C5154B0D5CBC9* __this, const RuntimeMethod* method) 
{
	{
		__this->____allowRenegotiation = (bool)1;
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2(__this, NULL);
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AuthenticatedStream__ctor_m8B163146A01B938A3E87E1BED07EE07494A3E357 (AuthenticatedStream_t8DCF41E151F705E2494FC7836F5E2EF7C539FA39* __this, Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* ___0_innerStream, bool ___1_leaveInnerStreamOpen, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_runtime_class_init_inline(Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE_il2cpp_TypeInfo_var);
		Stream__ctor_mE8B074A0EBEB026FFF14062AB4B8A78E17EFFBF0(__this, NULL);
		Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* L_0 = ___0_innerStream;
		if (!L_0)
		{
			goto IL_0011;
		}
	}
	{
		Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* L_1 = ___0_innerStream;
		il2cpp_codegen_runtime_class_init_inline(Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE_il2cpp_TypeInfo_var);
		Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* L_2 = ((Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE_StaticFields*)il2cpp_codegen_static_fields_for(Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE_il2cpp_TypeInfo_var))->___Null;
		if ((!(((RuntimeObject*)(Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE*)L_1) == ((RuntimeObject*)(Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE*)L_2))))
		{
			goto IL_001c;
		}
	}

IL_0011:
	{
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_3 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_3, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralA6EAFAF4139980ACB770E237272125EB9180B7A6)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_3, ((RuntimeMethod*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&AuthenticatedStream__ctor_m8B163146A01B938A3E87E1BED07EE07494A3E357_RuntimeMethod_var)));
	}

IL_001c:
	{
		Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* L_4 = ___0_innerStream;
		NullCheck(L_4);
		il2cpp_codegen_runtime_class_init_inline(Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE_il2cpp_TypeInfo_var);
		bool L_5;
		L_5 = VirtualFuncInvoker0< bool >::Invoke(7, L_4);
		if (!L_5)
		{
			goto IL_002c;
		}
	}
	{
		Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* L_6 = ___0_innerStream;
		NullCheck(L_6);
		il2cpp_codegen_runtime_class_init_inline(Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE_il2cpp_TypeInfo_var);
		bool L_7;
		L_7 = VirtualFuncInvoker0< bool >::Invoke(9, L_6);
		if (L_7)
		{
			goto IL_0041;
		}
	}

IL_002c:
	{
		String_t* L_8;
		L_8 = SR_GetString_m2F0EFCFA5FE3DB6DE89C5823284302D691355AA6(((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral1F40F39B9F659411903AC93DF93E8658291E5522)), NULL);
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_9 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m8F9D40CE19D19B698A70F9A258640EB52DB39B62(L_9, L_8, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralA6EAFAF4139980ACB770E237272125EB9180B7A6)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_9, ((RuntimeMethod*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&AuthenticatedStream__ctor_m8B163146A01B938A3E87E1BED07EE07494A3E357_RuntimeMethod_var)));
	}

IL_0041:
	{
		Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* L_10 = ___0_innerStream;
		__this->____InnerStream = L_10;
		Il2CppCodeGenWriteBarrier((void**)(&__this->____InnerStream), (void*)L_10);
		bool L_11 = ___1_leaveInnerStreamOpen;
		__this->____LeaveStreamOpen = L_11;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* AuthenticatedStream_get_InnerStream_m5BDA4857898A90C696DAD90FD61C536FC3EB10C7 (AuthenticatedStream_t8DCF41E151F705E2494FC7836F5E2EF7C539FA39* __this, const RuntimeMethod* method) 
{
	{
		Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* L_0 = __this->____InnerStream;
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AuthenticatedStream_Dispose_m031F600EE876A06427FB0713D992FF02B9C3320F (AuthenticatedStream_t8DCF41E151F705E2494FC7836F5E2EF7C539FA39* __this, bool ___0_disposing, const RuntimeMethod* method) 
{
	{
		auto __finallyBlock = il2cpp::utils::Finally([&]
		{

FINALLY_0025:
			{
				bool L_0 = ___0_disposing;
				Stream_Dispose_m9B37BD21A57F8F2BD20EE353DE14405700810C5C(__this, L_0, NULL);
				return;
			}
		});
		try
		{
			{
				bool L_1 = ___0_disposing;
				if (!L_1)
				{
					goto IL_0023_1;
				}
			}
			{
				bool L_2 = __this->____LeaveStreamOpen;
				if (!L_2)
				{
					goto IL_0018_1;
				}
			}
			{
				Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* L_3 = __this->____InnerStream;
				NullCheck(L_3);
				VirtualActionInvoker0::Invoke(15, L_3);
				goto IL_002d;
			}

IL_0018_1:
			{
				Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* L_4 = __this->____InnerStream;
				NullCheck(L_4);
				VirtualActionInvoker0::Invoke(13, L_4);
			}

IL_0023_1:
			{
				goto IL_002d;
			}
		}
		catch(Il2CppExceptionWrapper& e)
		{
			__finallyBlock.StoreException(e.ex);
		}
	}

IL_002d:
	{
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
bool RemoteCertificateValidationCallback_Invoke_m5256BF7D3B9BFFFAA959191FD5117C393F670D83_Multicast(RemoteCertificateValidationCallback_t2F4C5801F96B2C2BF934511796C5BFEAEBF01955* __this, RuntimeObject* ___0_sender, X509Certificate_t966CC553AF25AE7991F5B4C2AACBCF6C66C8F9C4* ___1_certificate, X509Chain_t2E7F88A4C97CAB82AF938F13A11354A0475469E5* ___2_chain, int32_t ___3_sslPolicyErrors, const RuntimeMethod* method)
{
	il2cpp_array_size_t length = __this->___delegates->max_length;
	Delegate_t** delegatesToInvoke = reinterpret_cast<Delegate_t**>(__this->___delegates->GetAddressAtUnchecked(0));
	bool retVal = false;
	for (il2cpp_array_size_t i = 0; i < length; i++)
	{
		RemoteCertificateValidationCallback_t2F4C5801F96B2C2BF934511796C5BFEAEBF01955* currentDelegate = reinterpret_cast<RemoteCertificateValidationCallback_t2F4C5801F96B2C2BF934511796C5BFEAEBF01955*>(delegatesToInvoke[i]);
		typedef bool (*FunctionPointerType) (RuntimeObject*, RuntimeObject*, X509Certificate_t966CC553AF25AE7991F5B4C2AACBCF6C66C8F9C4*, X509Chain_t2E7F88A4C97CAB82AF938F13A11354A0475469E5*, int32_t, const RuntimeMethod*);
		retVal = ((FunctionPointerType)currentDelegate->___invoke_impl)((Il2CppObject*)currentDelegate->___method_code, ___0_sender, ___1_certificate, ___2_chain, ___3_sslPolicyErrors, reinterpret_cast<RuntimeMethod*>(currentDelegate->___method));
	}
	return retVal;
}
bool RemoteCertificateValidationCallback_Invoke_m5256BF7D3B9BFFFAA959191FD5117C393F670D83_OpenInst(RemoteCertificateValidationCallback_t2F4C5801F96B2C2BF934511796C5BFEAEBF01955* __this, RuntimeObject* ___0_sender, X509Certificate_t966CC553AF25AE7991F5B4C2AACBCF6C66C8F9C4* ___1_certificate, X509Chain_t2E7F88A4C97CAB82AF938F13A11354A0475469E5* ___2_chain, int32_t ___3_sslPolicyErrors, const RuntimeMethod* method)
{
	NullCheck(___0_sender);
	typedef bool (*FunctionPointerType) (RuntimeObject*, X509Certificate_t966CC553AF25AE7991F5B4C2AACBCF6C66C8F9C4*, X509Chain_t2E7F88A4C97CAB82AF938F13A11354A0475469E5*, int32_t, const RuntimeMethod*);
	return ((FunctionPointerType)__this->___method_ptr)(___0_sender, ___1_certificate, ___2_chain, ___3_sslPolicyErrors, method);
}
bool RemoteCertificateValidationCallback_Invoke_m5256BF7D3B9BFFFAA959191FD5117C393F670D83_OpenStatic(RemoteCertificateValidationCallback_t2F4C5801F96B2C2BF934511796C5BFEAEBF01955* __this, RuntimeObject* ___0_sender, X509Certificate_t966CC553AF25AE7991F5B4C2AACBCF6C66C8F9C4* ___1_certificate, X509Chain_t2E7F88A4C97CAB82AF938F13A11354A0475469E5* ___2_chain, int32_t ___3_sslPolicyErrors, const RuntimeMethod* method)
{
	typedef bool (*FunctionPointerType) (RuntimeObject*, X509Certificate_t966CC553AF25AE7991F5B4C2AACBCF6C66C8F9C4*, X509Chain_t2E7F88A4C97CAB82AF938F13A11354A0475469E5*, int32_t, const RuntimeMethod*);
	return ((FunctionPointerType)__this->___method_ptr)(___0_sender, ___1_certificate, ___2_chain, ___3_sslPolicyErrors, method);
}
bool RemoteCertificateValidationCallback_Invoke_m5256BF7D3B9BFFFAA959191FD5117C393F670D83_OpenVirtual(RemoteCertificateValidationCallback_t2F4C5801F96B2C2BF934511796C5BFEAEBF01955* __this, RuntimeObject* ___0_sender, X509Certificate_t966CC553AF25AE7991F5B4C2AACBCF6C66C8F9C4* ___1_certificate, X509Chain_t2E7F88A4C97CAB82AF938F13A11354A0475469E5* ___2_chain, int32_t ___3_sslPolicyErrors, const RuntimeMethod* method)
{
	NullCheck(___0_sender);
	return VirtualFuncInvoker3< bool, X509Certificate_t966CC553AF25AE7991F5B4C2AACBCF6C66C8F9C4*, X509Chain_t2E7F88A4C97CAB82AF938F13A11354A0475469E5*, int32_t >::Invoke(il2cpp_codegen_method_get_slot(method), ___0_sender, ___1_certificate, ___2_chain, ___3_sslPolicyErrors);
}
bool RemoteCertificateValidationCallback_Invoke_m5256BF7D3B9BFFFAA959191FD5117C393F670D83_OpenInterface(RemoteCertificateValidationCallback_t2F4C5801F96B2C2BF934511796C5BFEAEBF01955* __this, RuntimeObject* ___0_sender, X509Certificate_t966CC553AF25AE7991F5B4C2AACBCF6C66C8F9C4* ___1_certificate, X509Chain_t2E7F88A4C97CAB82AF938F13A11354A0475469E5* ___2_chain, int32_t ___3_sslPolicyErrors, const RuntimeMethod* method)
{
	NullCheck(___0_sender);
	return InterfaceFuncInvoker3< bool, X509Certificate_t966CC553AF25AE7991F5B4C2AACBCF6C66C8F9C4*, X509Chain_t2E7F88A4C97CAB82AF938F13A11354A0475469E5*, int32_t >::Invoke(il2cpp_codegen_method_get_slot(method), il2cpp_codegen_method_get_declaring_type(method), ___0_sender, ___1_certificate, ___2_chain, ___3_sslPolicyErrors);
}
bool RemoteCertificateValidationCallback_Invoke_m5256BF7D3B9BFFFAA959191FD5117C393F670D83_OpenGenericVirtual(RemoteCertificateValidationCallback_t2F4C5801F96B2C2BF934511796C5BFEAEBF01955* __this, RuntimeObject* ___0_sender, X509Certificate_t966CC553AF25AE7991F5B4C2AACBCF6C66C8F9C4* ___1_certificate, X509Chain_t2E7F88A4C97CAB82AF938F13A11354A0475469E5* ___2_chain, int32_t ___3_sslPolicyErrors, const RuntimeMethod* method)
{
	NullCheck(___0_sender);
	return GenericVirtualFuncInvoker3< bool, X509Certificate_t966CC553AF25AE7991F5B4C2AACBCF6C66C8F9C4*, X509Chain_t2E7F88A4C97CAB82AF938F13A11354A0475469E5*, int32_t >::Invoke(method, ___0_sender, ___1_certificate, ___2_chain, ___3_sslPolicyErrors);
}
bool RemoteCertificateValidationCallback_Invoke_m5256BF7D3B9BFFFAA959191FD5117C393F670D83_OpenGenericInterface(RemoteCertificateValidationCallback_t2F4C5801F96B2C2BF934511796C5BFEAEBF01955* __this, RuntimeObject* ___0_sender, X509Certificate_t966CC553AF25AE7991F5B4C2AACBCF6C66C8F9C4* ___1_certificate, X509Chain_t2E7F88A4C97CAB82AF938F13A11354A0475469E5* ___2_chain, int32_t ___3_sslPolicyErrors, const RuntimeMethod* method)
{
	NullCheck(___0_sender);
	return GenericInterfaceFuncInvoker3< bool, X509Certificate_t966CC553AF25AE7991F5B4C2AACBCF6C66C8F9C4*, X509Chain_t2E7F88A4C97CAB82AF938F13A11354A0475469E5*, int32_t >::Invoke(method, ___0_sender, ___1_certificate, ___2_chain, ___3_sslPolicyErrors);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void RemoteCertificateValidationCallback__ctor_mC5C0650D4E5D29475BDBDA5B43FEA44DB0DBF367 (RemoteCertificateValidationCallback_t2F4C5801F96B2C2BF934511796C5BFEAEBF01955* __this, RuntimeObject* ___0_object, intptr_t ___1_method, const RuntimeMethod* method) 
{
	__this->___method_ptr = (intptr_t)il2cpp_codegen_get_method_pointer((RuntimeMethod*)___1_method);
	__this->___method = ___1_method;
	__this->___m_target = ___0_object;
	Il2CppCodeGenWriteBarrier((void**)(&__this->___m_target), (void*)___0_object);
	int parameterCount = il2cpp_codegen_method_parameter_count((RuntimeMethod*)___1_method);
	__this->___method_code = (intptr_t)__this;
	if (MethodIsStatic((RuntimeMethod*)___1_method))
	{
		bool isOpen = parameterCount == 4;
		if (isOpen)
			__this->___invoke_impl = (intptr_t)&RemoteCertificateValidationCallback_Invoke_m5256BF7D3B9BFFFAA959191FD5117C393F670D83_OpenStatic;
		else
			{
				__this->___invoke_impl = __this->___method_ptr;
				__this->___method_code = (intptr_t)__this->___m_target;
			}
	}
	else
	{
		bool isOpen = parameterCount == 3;
		if (isOpen)
		{
			if (__this->___method_is_virtual)
			{
				if (il2cpp_codegen_method_is_generic_instance_method((RuntimeMethod*)___1_method))
					if (il2cpp_codegen_method_is_interface_method((RuntimeMethod*)___1_method))
						__this->___invoke_impl = (intptr_t)&RemoteCertificateValidationCallback_Invoke_m5256BF7D3B9BFFFAA959191FD5117C393F670D83_OpenGenericInterface;
					else
						__this->___invoke_impl = (intptr_t)&RemoteCertificateValidationCallback_Invoke_m5256BF7D3B9BFFFAA959191FD5117C393F670D83_OpenGenericVirtual;
				else
					if (il2cpp_codegen_method_is_interface_method((RuntimeMethod*)___1_method))
						__this->___invoke_impl = (intptr_t)&RemoteCertificateValidationCallback_Invoke_m5256BF7D3B9BFFFAA959191FD5117C393F670D83_OpenInterface;
					else
						__this->___invoke_impl = (intptr_t)&RemoteCertificateValidationCallback_Invoke_m5256BF7D3B9BFFFAA959191FD5117C393F670D83_OpenVirtual;
			}
			else
			{
				__this->___invoke_impl = (intptr_t)&RemoteCertificateValidationCallback_Invoke_m5256BF7D3B9BFFFAA959191FD5117C393F670D83_OpenInst;
			}
		}
		else
		{
			if (___0_object == NULL)
				il2cpp_codegen_raise_exception(il2cpp_codegen_get_argument_exception(NULL, "Delegate to an instance method cannot have null 'this'."), NULL);
			__this->___invoke_impl = __this->___method_ptr;
			__this->___method_code = (intptr_t)__this->___m_target;
		}
	}
	__this->___extra_arg = (intptr_t)&RemoteCertificateValidationCallback_Invoke_m5256BF7D3B9BFFFAA959191FD5117C393F670D83_Multicast;
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool RemoteCertificateValidationCallback_Invoke_m5256BF7D3B9BFFFAA959191FD5117C393F670D83 (RemoteCertificateValidationCallback_t2F4C5801F96B2C2BF934511796C5BFEAEBF01955* __this, RuntimeObject* ___0_sender, X509Certificate_t966CC553AF25AE7991F5B4C2AACBCF6C66C8F9C4* ___1_certificate, X509Chain_t2E7F88A4C97CAB82AF938F13A11354A0475469E5* ___2_chain, int32_t ___3_sslPolicyErrors, const RuntimeMethod* method) 
{
	typedef bool (*FunctionPointerType) (RuntimeObject*, RuntimeObject*, X509Certificate_t966CC553AF25AE7991F5B4C2AACBCF6C66C8F9C4*, X509Chain_t2E7F88A4C97CAB82AF938F13A11354A0475469E5*, int32_t, const RuntimeMethod*);
	return ((FunctionPointerType)__this->___invoke_impl)((Il2CppObject*)__this->___method_code, ___0_sender, ___1_certificate, ___2_chain, ___3_sslPolicyErrors, reinterpret_cast<RuntimeMethod*>(__this->___method));
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
X509Certificate_t966CC553AF25AE7991F5B4C2AACBCF6C66C8F9C4* LocalCertSelectionCallback_Invoke_m3AD956B5D53937117BD3E036068B49615337B5B9_Multicast(LocalCertSelectionCallback_t3AB79A8AA642074A3EF886118963EEA3CCABABE2* __this, String_t* ___0_targetHost, X509CertificateCollection_t2900D71D188EDCA7DEB5077D36103EE5DA6805CE* ___1_localCertificates, X509Certificate_t966CC553AF25AE7991F5B4C2AACBCF6C66C8F9C4* ___2_remoteCertificate, StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* ___3_acceptableIssuers, const RuntimeMethod* method)
{
	il2cpp_array_size_t length = __this->___delegates->max_length;
	Delegate_t** delegatesToInvoke = reinterpret_cast<Delegate_t**>(__this->___delegates->GetAddressAtUnchecked(0));
	X509Certificate_t966CC553AF25AE7991F5B4C2AACBCF6C66C8F9C4* retVal = NULL;
	for (il2cpp_array_size_t i = 0; i < length; i++)
	{
		LocalCertSelectionCallback_t3AB79A8AA642074A3EF886118963EEA3CCABABE2* currentDelegate = reinterpret_cast<LocalCertSelectionCallback_t3AB79A8AA642074A3EF886118963EEA3CCABABE2*>(delegatesToInvoke[i]);
		typedef X509Certificate_t966CC553AF25AE7991F5B4C2AACBCF6C66C8F9C4* (*FunctionPointerType) (RuntimeObject*, String_t*, X509CertificateCollection_t2900D71D188EDCA7DEB5077D36103EE5DA6805CE*, X509Certificate_t966CC553AF25AE7991F5B4C2AACBCF6C66C8F9C4*, StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248*, const RuntimeMethod*);
		retVal = ((FunctionPointerType)currentDelegate->___invoke_impl)((Il2CppObject*)currentDelegate->___method_code, ___0_targetHost, ___1_localCertificates, ___2_remoteCertificate, ___3_acceptableIssuers, reinterpret_cast<RuntimeMethod*>(currentDelegate->___method));
	}
	return retVal;
}
X509Certificate_t966CC553AF25AE7991F5B4C2AACBCF6C66C8F9C4* LocalCertSelectionCallback_Invoke_m3AD956B5D53937117BD3E036068B49615337B5B9_OpenInst(LocalCertSelectionCallback_t3AB79A8AA642074A3EF886118963EEA3CCABABE2* __this, String_t* ___0_targetHost, X509CertificateCollection_t2900D71D188EDCA7DEB5077D36103EE5DA6805CE* ___1_localCertificates, X509Certificate_t966CC553AF25AE7991F5B4C2AACBCF6C66C8F9C4* ___2_remoteCertificate, StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* ___3_acceptableIssuers, const RuntimeMethod* method)
{
	NullCheck(___0_targetHost);
	typedef X509Certificate_t966CC553AF25AE7991F5B4C2AACBCF6C66C8F9C4* (*FunctionPointerType) (String_t*, X509CertificateCollection_t2900D71D188EDCA7DEB5077D36103EE5DA6805CE*, X509Certificate_t966CC553AF25AE7991F5B4C2AACBCF6C66C8F9C4*, StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248*, const RuntimeMethod*);
	return ((FunctionPointerType)__this->___method_ptr)(___0_targetHost, ___1_localCertificates, ___2_remoteCertificate, ___3_acceptableIssuers, method);
}
X509Certificate_t966CC553AF25AE7991F5B4C2AACBCF6C66C8F9C4* LocalCertSelectionCallback_Invoke_m3AD956B5D53937117BD3E036068B49615337B5B9_OpenStatic(LocalCertSelectionCallback_t3AB79A8AA642074A3EF886118963EEA3CCABABE2* __this, String_t* ___0_targetHost, X509CertificateCollection_t2900D71D188EDCA7DEB5077D36103EE5DA6805CE* ___1_localCertificates, X509Certificate_t966CC553AF25AE7991F5B4C2AACBCF6C66C8F9C4* ___2_remoteCertificate, StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* ___3_acceptableIssuers, const RuntimeMethod* method)
{
	typedef X509Certificate_t966CC553AF25AE7991F5B4C2AACBCF6C66C8F9C4* (*FunctionPointerType) (String_t*, X509CertificateCollection_t2900D71D188EDCA7DEB5077D36103EE5DA6805CE*, X509Certificate_t966CC553AF25AE7991F5B4C2AACBCF6C66C8F9C4*, StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248*, const RuntimeMethod*);
	return ((FunctionPointerType)__this->___method_ptr)(___0_targetHost, ___1_localCertificates, ___2_remoteCertificate, ___3_acceptableIssuers, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void LocalCertSelectionCallback__ctor_mA7B36924E69A918A71649CFF95CB361E33317C38 (LocalCertSelectionCallback_t3AB79A8AA642074A3EF886118963EEA3CCABABE2* __this, RuntimeObject* ___0_object, intptr_t ___1_method, const RuntimeMethod* method) 
{
	__this->___method_ptr = (intptr_t)il2cpp_codegen_get_method_pointer((RuntimeMethod*)___1_method);
	__this->___method = ___1_method;
	__this->___m_target = ___0_object;
	Il2CppCodeGenWriteBarrier((void**)(&__this->___m_target), (void*)___0_object);
	int parameterCount = il2cpp_codegen_method_parameter_count((RuntimeMethod*)___1_method);
	__this->___method_code = (intptr_t)__this;
	if (MethodIsStatic((RuntimeMethod*)___1_method))
	{
		bool isOpen = parameterCount == 4;
		if (isOpen)
			__this->___invoke_impl = (intptr_t)&LocalCertSelectionCallback_Invoke_m3AD956B5D53937117BD3E036068B49615337B5B9_OpenStatic;
		else
			{
				__this->___invoke_impl = __this->___method_ptr;
				__this->___method_code = (intptr_t)__this->___m_target;
			}
	}
	else
	{
		bool isOpen = parameterCount == 3;
		if (isOpen)
		{
			__this->___invoke_impl = (intptr_t)&LocalCertSelectionCallback_Invoke_m3AD956B5D53937117BD3E036068B49615337B5B9_OpenInst;
		}
		else
		{
			if (___0_object == NULL)
				il2cpp_codegen_raise_exception(il2cpp_codegen_get_argument_exception(NULL, "Delegate to an instance method cannot have null 'this'."), NULL);
			__this->___invoke_impl = __this->___method_ptr;
			__this->___method_code = (intptr_t)__this->___m_target;
		}
	}
	__this->___extra_arg = (intptr_t)&LocalCertSelectionCallback_Invoke_m3AD956B5D53937117BD3E036068B49615337B5B9_Multicast;
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR X509Certificate_t966CC553AF25AE7991F5B4C2AACBCF6C66C8F9C4* LocalCertSelectionCallback_Invoke_m3AD956B5D53937117BD3E036068B49615337B5B9 (LocalCertSelectionCallback_t3AB79A8AA642074A3EF886118963EEA3CCABABE2* __this, String_t* ___0_targetHost, X509CertificateCollection_t2900D71D188EDCA7DEB5077D36103EE5DA6805CE* ___1_localCertificates, X509Certificate_t966CC553AF25AE7991F5B4C2AACBCF6C66C8F9C4* ___2_remoteCertificate, StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* ___3_acceptableIssuers, const RuntimeMethod* method) 
{
	typedef X509Certificate_t966CC553AF25AE7991F5B4C2AACBCF6C66C8F9C4* (*FunctionPointerType) (RuntimeObject*, String_t*, X509CertificateCollection_t2900D71D188EDCA7DEB5077D36103EE5DA6805CE*, X509Certificate_t966CC553AF25AE7991F5B4C2AACBCF6C66C8F9C4*, StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248*, const RuntimeMethod*);
	return ((FunctionPointerType)__this->___invoke_impl)((Il2CppObject*)__this->___method_code, ___0_targetHost, ___1_localCertificates, ___2_remoteCertificate, ___3_acceptableIssuers, reinterpret_cast<RuntimeMethod*>(__this->___method));
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
X509Certificate_t966CC553AF25AE7991F5B4C2AACBCF6C66C8F9C4* ServerCertSelectionCallback_Invoke_mB6A207E1A8CA76F667A242B68943297741CC6EC1_Multicast(ServerCertSelectionCallback_t653386CAEAE0236FCF61A92963AB1646BB23C654* __this, String_t* ___0_hostName, const RuntimeMethod* method)
{
	il2cpp_array_size_t length = __this->___delegates->max_length;
	Delegate_t** delegatesToInvoke = reinterpret_cast<Delegate_t**>(__this->___delegates->GetAddressAtUnchecked(0));
	X509Certificate_t966CC553AF25AE7991F5B4C2AACBCF6C66C8F9C4* retVal = NULL;
	for (il2cpp_array_size_t i = 0; i < length; i++)
	{
		ServerCertSelectionCallback_t653386CAEAE0236FCF61A92963AB1646BB23C654* currentDelegate = reinterpret_cast<ServerCertSelectionCallback_t653386CAEAE0236FCF61A92963AB1646BB23C654*>(delegatesToInvoke[i]);
		typedef X509Certificate_t966CC553AF25AE7991F5B4C2AACBCF6C66C8F9C4* (*FunctionPointerType) (RuntimeObject*, String_t*, const RuntimeMethod*);
		retVal = ((FunctionPointerType)currentDelegate->___invoke_impl)((Il2CppObject*)currentDelegate->___method_code, ___0_hostName, reinterpret_cast<RuntimeMethod*>(currentDelegate->___method));
	}
	return retVal;
}
X509Certificate_t966CC553AF25AE7991F5B4C2AACBCF6C66C8F9C4* ServerCertSelectionCallback_Invoke_mB6A207E1A8CA76F667A242B68943297741CC6EC1_OpenInst(ServerCertSelectionCallback_t653386CAEAE0236FCF61A92963AB1646BB23C654* __this, String_t* ___0_hostName, const RuntimeMethod* method)
{
	NullCheck(___0_hostName);
	typedef X509Certificate_t966CC553AF25AE7991F5B4C2AACBCF6C66C8F9C4* (*FunctionPointerType) (String_t*, const RuntimeMethod*);
	return ((FunctionPointerType)__this->___method_ptr)(___0_hostName, method);
}
X509Certificate_t966CC553AF25AE7991F5B4C2AACBCF6C66C8F9C4* ServerCertSelectionCallback_Invoke_mB6A207E1A8CA76F667A242B68943297741CC6EC1_OpenStatic(ServerCertSelectionCallback_t653386CAEAE0236FCF61A92963AB1646BB23C654* __this, String_t* ___0_hostName, const RuntimeMethod* method)
{
	typedef X509Certificate_t966CC553AF25AE7991F5B4C2AACBCF6C66C8F9C4* (*FunctionPointerType) (String_t*, const RuntimeMethod*);
	return ((FunctionPointerType)__this->___method_ptr)(___0_hostName, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ServerCertSelectionCallback__ctor_m06DDCEDF49D7A5F0F8494E3E960C7DFC64C64946 (ServerCertSelectionCallback_t653386CAEAE0236FCF61A92963AB1646BB23C654* __this, RuntimeObject* ___0_object, intptr_t ___1_method, const RuntimeMethod* method) 
{
	__this->___method_ptr = (intptr_t)il2cpp_codegen_get_method_pointer((RuntimeMethod*)___1_method);
	__this->___method = ___1_method;
	__this->___m_target = ___0_object;
	Il2CppCodeGenWriteBarrier((void**)(&__this->___m_target), (void*)___0_object);
	int parameterCount = il2cpp_codegen_method_parameter_count((RuntimeMethod*)___1_method);
	__this->___method_code = (intptr_t)__this;
	if (MethodIsStatic((RuntimeMethod*)___1_method))
	{
		bool isOpen = parameterCount == 1;
		if (isOpen)
			__this->___invoke_impl = (intptr_t)&ServerCertSelectionCallback_Invoke_mB6A207E1A8CA76F667A242B68943297741CC6EC1_OpenStatic;
		else
			{
				__this->___invoke_impl = __this->___method_ptr;
				__this->___method_code = (intptr_t)__this->___m_target;
			}
	}
	else
	{
		bool isOpen = parameterCount == 0;
		if (isOpen)
		{
			__this->___invoke_impl = (intptr_t)&ServerCertSelectionCallback_Invoke_mB6A207E1A8CA76F667A242B68943297741CC6EC1_OpenInst;
		}
		else
		{
			if (___0_object == NULL)
				il2cpp_codegen_raise_exception(il2cpp_codegen_get_argument_exception(NULL, "Delegate to an instance method cannot have null 'this'."), NULL);
			__this->___invoke_impl = __this->___method_ptr;
			__this->___method_code = (intptr_t)__this->___m_target;
		}
	}
	__this->___extra_arg = (intptr_t)&ServerCertSelectionCallback_Invoke_mB6A207E1A8CA76F667A242B68943297741CC6EC1_Multicast;
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR X509Certificate_t966CC553AF25AE7991F5B4C2AACBCF6C66C8F9C4* ServerCertSelectionCallback_Invoke_mB6A207E1A8CA76F667A242B68943297741CC6EC1 (ServerCertSelectionCallback_t653386CAEAE0236FCF61A92963AB1646BB23C654* __this, String_t* ___0_hostName, const RuntimeMethod* method) 
{
	typedef X509Certificate_t966CC553AF25AE7991F5B4C2AACBCF6C66C8F9C4* (*FunctionPointerType) (RuntimeObject*, String_t*, const RuntimeMethod*);
	return ((FunctionPointerType)__this->___invoke_impl)((Il2CppObject*)__this->___method_code, ___0_hostName, reinterpret_cast<RuntimeMethod*>(__this->___method));
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR MobileAuthenticatedStream_tD0306DC2B0CDA3C7DB261C19FFA35CA8EE24309E* SslStream_get_Impl_m1356F55D556B6CBE922FB053F79BC930FCADF057 (SslStream_t19A079881850F9CAD7BAA6FB625BBC4647ED5A27* __this, const RuntimeMethod* method) 
{
	{
		SslStream_CheckDisposed_m884E18F119B366EE6734CCFA126A1F633687FA95(__this, NULL);
		MobileAuthenticatedStream_tD0306DC2B0CDA3C7DB261C19FFA35CA8EE24309E* L_0 = __this->___impl;
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* SslStream_get_InternalTargetHost_m9434AC1422D463C25B5C5313AC9BE45B095136AE (SslStream_t19A079881850F9CAD7BAA6FB625BBC4647ED5A27* __this, const RuntimeMethod* method) 
{
	{
		SslStream_CheckDisposed_m884E18F119B366EE6734CCFA126A1F633687FA95(__this, NULL);
		MobileAuthenticatedStream_tD0306DC2B0CDA3C7DB261C19FFA35CA8EE24309E* L_0 = __this->___impl;
		NullCheck(L_0);
		String_t* L_1;
		L_1 = MobileAuthenticatedStream_get_TargetHost_m572DA701CB3CE3FA4006D5745F9A3B9A52739CAB_inline(L_0, NULL);
		return L_1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR MobileTlsProvider_tD60D82BEBF267F50F388A026DBB092C7188BB017* SslStream_GetProvider_mE1D070C13DB759156659F57AB9DF4D2E397FD85B (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&MobileTlsProvider_tD60D82BEBF267F50F388A026DBB092C7188BB017_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		MonoTlsProvider_t39C898CDC9458EEAD7C019B4B23701EAF9E24F7E* L_0;
		L_0 = MonoTlsProviderFactory_GetProvider_m8684E3A1AFB043FA00DEC4BCF95F8B288C136936(NULL);
		return ((MobileTlsProvider_tD60D82BEBF267F50F388A026DBB092C7188BB017*)CastclassClass((RuntimeObject*)L_0, MobileTlsProvider_tD60D82BEBF267F50F388A026DBB092C7188BB017_il2cpp_TypeInfo_var));
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SslStream__ctor_m710281F6BF541BC6796ECE848E0CE87A52C3B1C1 (SslStream_t19A079881850F9CAD7BAA6FB625BBC4647ED5A27* __this, Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* ___0_innerStream, const RuntimeMethod* method) 
{
	{
		Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* L_0 = ___0_innerStream;
		SslStream__ctor_m91391419C92963CB7B2D1F1DAB448843CD07681B(__this, L_0, (bool)0, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SslStream__ctor_m91391419C92963CB7B2D1F1DAB448843CD07681B (SslStream_t19A079881850F9CAD7BAA6FB625BBC4647ED5A27* __this, Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* ___0_innerStream, bool ___1_leaveInnerStreamOpen, const RuntimeMethod* method) 
{
	{
		Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* L_0 = ___0_innerStream;
		bool L_1 = ___1_leaveInnerStreamOpen;
		AuthenticatedStream__ctor_m8B163146A01B938A3E87E1BED07EE07494A3E357(__this, L_0, L_1, NULL);
		MobileTlsProvider_tD60D82BEBF267F50F388A026DBB092C7188BB017* L_2;
		L_2 = SslStream_GetProvider_mE1D070C13DB759156659F57AB9DF4D2E397FD85B(NULL);
		__this->___provider = L_2;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___provider), (void*)L_2);
		MonoTlsSettings_tD79AF4AE5C2CD533A3D7A08FED479B1EC1A031B0* L_3;
		L_3 = MonoTlsSettings_CopyDefaultSettings_m4B0A3E8B7D106FA7F0D243FB2A0A4B115CD21942(NULL);
		__this->___settings = L_3;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___settings), (void*)L_3);
		MobileTlsProvider_tD60D82BEBF267F50F388A026DBB092C7188BB017* L_4 = __this->___provider;
		Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* L_5 = ___0_innerStream;
		bool L_6 = ___1_leaveInnerStreamOpen;
		MonoTlsSettings_tD79AF4AE5C2CD533A3D7A08FED479B1EC1A031B0* L_7 = __this->___settings;
		NullCheck(L_4);
		MobileAuthenticatedStream_tD0306DC2B0CDA3C7DB261C19FFA35CA8EE24309E* L_8;
		L_8 = VirtualFuncInvoker4< MobileAuthenticatedStream_tD0306DC2B0CDA3C7DB261C19FFA35CA8EE24309E*, SslStream_t19A079881850F9CAD7BAA6FB625BBC4647ED5A27*, Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE*, bool, MonoTlsSettings_tD79AF4AE5C2CD533A3D7A08FED479B1EC1A031B0* >::Invoke(11, L_4, __this, L_5, L_6, L_7);
		__this->___impl = L_8;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___impl), (void*)L_8);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Task_t751C4CC3ECD055BABA8A0B6A5DFBB4283DCA8572* SslStream_AuthenticateAsClientAsync_mAD6DEB8ADE693F969D039B19C19A85F13A2DCB23 (SslStream_t19A079881850F9CAD7BAA6FB625BBC4647ED5A27* __this, String_t* ___0_targetHost, X509CertificateCollection_t2900D71D188EDCA7DEB5077D36103EE5DA6805CE* ___1_clientCertificates, int32_t ___2_enabledSslProtocols, bool ___3_checkCertificateRevocation, const RuntimeMethod* method) 
{
	{
		MobileAuthenticatedStream_tD0306DC2B0CDA3C7DB261C19FFA35CA8EE24309E* L_0;
		L_0 = SslStream_get_Impl_m1356F55D556B6CBE922FB053F79BC930FCADF057(__this, NULL);
		String_t* L_1 = ___0_targetHost;
		X509CertificateCollection_t2900D71D188EDCA7DEB5077D36103EE5DA6805CE* L_2 = ___1_clientCertificates;
		int32_t L_3 = ___2_enabledSslProtocols;
		bool L_4 = ___3_checkCertificateRevocation;
		NullCheck(L_0);
		Task_t751C4CC3ECD055BABA8A0B6A5DFBB4283DCA8572* L_5;
		L_5 = MobileAuthenticatedStream_AuthenticateAsClientAsync_mBD8A0E7FFAFDE608063E545EF057304AF2C35EF2(L_0, L_1, L_2, L_3, L_4, NULL);
		return L_5;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool SslStream_get_IsAuthenticated_m5056C714711944DA54B35F6F2A3025A720B49290 (SslStream_t19A079881850F9CAD7BAA6FB625BBC4647ED5A27* __this, const RuntimeMethod* method) 
{
	{
		MobileAuthenticatedStream_tD0306DC2B0CDA3C7DB261C19FFA35CA8EE24309E* L_0;
		L_0 = SslStream_get_Impl_m1356F55D556B6CBE922FB053F79BC930FCADF057(__this, NULL);
		NullCheck(L_0);
		bool L_1;
		L_1 = VirtualFuncInvoker0< bool >::Invoke(31, L_0);
		return L_1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool SslStream_get_CanSeek_mA98FD41EDB70B03DB44F714CE952E128CA9D9B4B (SslStream_t19A079881850F9CAD7BAA6FB625BBC4647ED5A27* __this, const RuntimeMethod* method) 
{
	{
		return (bool)0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool SslStream_get_CanRead_m0B2ED5711B4D7C796529F56D7039AD546571FEF2 (SslStream_t19A079881850F9CAD7BAA6FB625BBC4647ED5A27* __this, const RuntimeMethod* method) 
{
	{
		MobileAuthenticatedStream_tD0306DC2B0CDA3C7DB261C19FFA35CA8EE24309E* L_0 = __this->___impl;
		if (!L_0)
		{
			goto IL_0014;
		}
	}
	{
		MobileAuthenticatedStream_tD0306DC2B0CDA3C7DB261C19FFA35CA8EE24309E* L_1 = __this->___impl;
		NullCheck(L_1);
		bool L_2;
		L_2 = VirtualFuncInvoker0< bool >::Invoke(7, L_1);
		return L_2;
	}

IL_0014:
	{
		return (bool)0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool SslStream_get_CanWrite_mCBCABFAC940539066F34E4480E52E07CA06DA7CE (SslStream_t19A079881850F9CAD7BAA6FB625BBC4647ED5A27* __this, const RuntimeMethod* method) 
{
	{
		MobileAuthenticatedStream_tD0306DC2B0CDA3C7DB261C19FFA35CA8EE24309E* L_0 = __this->___impl;
		if (!L_0)
		{
			goto IL_0014;
		}
	}
	{
		MobileAuthenticatedStream_tD0306DC2B0CDA3C7DB261C19FFA35CA8EE24309E* L_1 = __this->___impl;
		NullCheck(L_1);
		bool L_2;
		L_2 = VirtualFuncInvoker0< bool >::Invoke(9, L_1);
		return L_2;
	}

IL_0014:
	{
		return (bool)0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t SslStream_get_Length_mD6515D77DBB57912C598761F67C321B6B16E129C (SslStream_t19A079881850F9CAD7BAA6FB625BBC4647ED5A27* __this, const RuntimeMethod* method) 
{
	{
		MobileAuthenticatedStream_tD0306DC2B0CDA3C7DB261C19FFA35CA8EE24309E* L_0;
		L_0 = SslStream_get_Impl_m1356F55D556B6CBE922FB053F79BC930FCADF057(__this, NULL);
		NullCheck(L_0);
		int64_t L_1;
		L_1 = VirtualFuncInvoker0< int64_t >::Invoke(10, L_0);
		return L_1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t SslStream_get_Position_mD2DBB36BA22F4620B635557B5748E0928F28B6A2 (SslStream_t19A079881850F9CAD7BAA6FB625BBC4647ED5A27* __this, const RuntimeMethod* method) 
{
	{
		MobileAuthenticatedStream_tD0306DC2B0CDA3C7DB261C19FFA35CA8EE24309E* L_0;
		L_0 = SslStream_get_Impl_m1356F55D556B6CBE922FB053F79BC930FCADF057(__this, NULL);
		NullCheck(L_0);
		int64_t L_1;
		L_1 = VirtualFuncInvoker0< int64_t >::Invoke(11, L_0);
		return L_1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SslStream_set_Position_m70987A87C7DB346D2FD447EA11E96774105EE5D2 (SslStream_t19A079881850F9CAD7BAA6FB625BBC4647ED5A27* __this, int64_t ___0_value, const RuntimeMethod* method) 
{
	{
		String_t* L_0;
		L_0 = SR_GetString_m2F0EFCFA5FE3DB6DE89C5823284302D691355AA6(((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral9A1C57A14DBAA067A98C8B33FA6B28D793AFE67D)), NULL);
		NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A* L_1 = (NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A_il2cpp_TypeInfo_var)));
		NotSupportedException__ctor_mE174750CF0247BBB47544FFD71D66BB89630945B(L_1, L_0, NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, ((RuntimeMethod*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&SslStream_set_Position_m70987A87C7DB346D2FD447EA11E96774105EE5D2_RuntimeMethod_var)));
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t SslStream_Seek_mFEDBF392998DDDB0F7A71F25D1BB821EA1463B25 (SslStream_t19A079881850F9CAD7BAA6FB625BBC4647ED5A27* __this, int64_t ___0_offset, int32_t ___1_origin, const RuntimeMethod* method) 
{
	{
		String_t* L_0;
		L_0 = SR_GetString_m2F0EFCFA5FE3DB6DE89C5823284302D691355AA6(((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral9A1C57A14DBAA067A98C8B33FA6B28D793AFE67D)), NULL);
		NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A* L_1 = (NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&NotSupportedException_t1429765983D409BD2986508963C98D214E4EBF4A_il2cpp_TypeInfo_var)));
		NotSupportedException__ctor_mE174750CF0247BBB47544FFD71D66BB89630945B(L_1, L_0, NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, ((RuntimeMethod*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&SslStream_Seek_mFEDBF392998DDDB0F7A71F25D1BB821EA1463B25_RuntimeMethod_var)));
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SslStream_Flush_m61B328FCFEB939DE23740823CE3EB7E8DF3B14A4 (SslStream_t19A079881850F9CAD7BAA6FB625BBC4647ED5A27* __this, const RuntimeMethod* method) 
{
	{
		Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* L_0;
		L_0 = AuthenticatedStream_get_InnerStream_m5BDA4857898A90C696DAD90FD61C536FC3EB10C7_inline(__this, NULL);
		NullCheck(L_0);
		VirtualActionInvoker0::Invoke(15, L_0);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SslStream_CheckDisposed_m884E18F119B366EE6734CCFA126A1F633687FA95 (SslStream_t19A079881850F9CAD7BAA6FB625BBC4647ED5A27* __this, const RuntimeMethod* method) 
{
	{
		MobileAuthenticatedStream_tD0306DC2B0CDA3C7DB261C19FFA35CA8EE24309E* L_0 = __this->___impl;
		if (L_0)
		{
			goto IL_0013;
		}
	}
	{
		ObjectDisposedException_tC5FB29E8E980E2010A2F6A5B9B791089419F89EB* L_1 = (ObjectDisposedException_tC5FB29E8E980E2010A2F6A5B9B791089419F89EB*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ObjectDisposedException_tC5FB29E8E980E2010A2F6A5B9B791089419F89EB_il2cpp_TypeInfo_var)));
		ObjectDisposedException__ctor_mB2C8582279AF3F0C1CF9AA52DA7331BF848DFD48(L_1, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral6FE6AA6EF03FB6550152903A7B201A6108F728DB)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, ((RuntimeMethod*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&SslStream_CheckDisposed_m884E18F119B366EE6734CCFA126A1F633687FA95_RuntimeMethod_var)));
	}

IL_0013:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SslStream_Dispose_m187B76F54E2471469F5B62B8DDB33E11E40EE397 (SslStream_t19A079881850F9CAD7BAA6FB625BBC4647ED5A27* __this, bool ___0_disposing, const RuntimeMethod* method) 
{
	{
		auto __finallyBlock = il2cpp::utils::Finally([&]
		{

FINALLY_0021:
			{
				bool L_0 = ___0_disposing;
				AuthenticatedStream_Dispose_m031F600EE876A06427FB0713D992FF02B9C3320F(__this, L_0, NULL);
				return;
			}
		});
		try
		{
			{
				MobileAuthenticatedStream_tD0306DC2B0CDA3C7DB261C19FFA35CA8EE24309E* L_1 = __this->___impl;
				bool L_2 = ___0_disposing;
				if (!((int32_t)(((!(((RuntimeObject*)(MobileAuthenticatedStream_tD0306DC2B0CDA3C7DB261C19FFA35CA8EE24309E*)L_1) <= ((RuntimeObject*)(RuntimeObject*)NULL)))? 1 : 0)&(int32_t)L_2)))
				{
					goto IL_001f_1;
				}
			}
			{
				MobileAuthenticatedStream_tD0306DC2B0CDA3C7DB261C19FFA35CA8EE24309E* L_3 = __this->___impl;
				NullCheck(L_3);
				Stream_Dispose_mCDB42F32A17541CCA6D3A5906827A401570B07A8(L_3, NULL);
				__this->___impl = (MobileAuthenticatedStream_tD0306DC2B0CDA3C7DB261C19FFA35CA8EE24309E*)NULL;
				Il2CppCodeGenWriteBarrier((void**)(&__this->___impl), (void*)(MobileAuthenticatedStream_tD0306DC2B0CDA3C7DB261C19FFA35CA8EE24309E*)NULL);
			}

IL_001f_1:
			{
				goto IL_0029;
			}
		}
		catch(Il2CppExceptionWrapper& e)
		{
			__finallyBlock.StoreException(e.ex);
		}
	}

IL_0029:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t SslStream_Read_mEAFC2AA192D9EAF401E2B067395EA08EDC5FB47E (SslStream_t19A079881850F9CAD7BAA6FB625BBC4647ED5A27* __this, ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* ___0_buffer, int32_t ___1_offset, int32_t ___2_count, const RuntimeMethod* method) 
{
	{
		MobileAuthenticatedStream_tD0306DC2B0CDA3C7DB261C19FFA35CA8EE24309E* L_0;
		L_0 = SslStream_get_Impl_m1356F55D556B6CBE922FB053F79BC930FCADF057(__this, NULL);
		ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* L_1 = ___0_buffer;
		int32_t L_2 = ___1_offset;
		int32_t L_3 = ___2_count;
		NullCheck(L_0);
		int32_t L_4;
		L_4 = VirtualFuncInvoker3< int32_t, ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031*, int32_t, int32_t >::Invoke(25, L_0, L_1, L_2, L_3);
		return L_4;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SslStream_Write_mCF650FDF725E1AAEDC462A016A38E535F7210F3C (SslStream_t19A079881850F9CAD7BAA6FB625BBC4647ED5A27* __this, ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* ___0_buffer, int32_t ___1_offset, int32_t ___2_count, const RuntimeMethod* method) 
{
	{
		MobileAuthenticatedStream_tD0306DC2B0CDA3C7DB261C19FFA35CA8EE24309E* L_0;
		L_0 = SslStream_get_Impl_m1356F55D556B6CBE922FB053F79BC930FCADF057(__this, NULL);
		ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* L_1 = ___0_buffer;
		int32_t L_2 = ___1_offset;
		int32_t L_3 = ___2_count;
		NullCheck(L_0);
		VirtualActionInvoker3< ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031*, int32_t, int32_t >::Invoke(28, L_0, L_1, L_2, L_3);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Task_1_t4C228DE57804012969575431CFF12D57C875552D* SslStream_ReadAsync_m0F251A72A8D1C8C41A49CC99AE55D8D0F96A956C (SslStream_t19A079881850F9CAD7BAA6FB625BBC4647ED5A27* __this, ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* ___0_buffer, int32_t ___1_offset, int32_t ___2_count, CancellationToken_t51142D9C6D7C02D314DA34A6A7988C528992FFED ___3_cancellationToken, const RuntimeMethod* method) 
{
	{
		MobileAuthenticatedStream_tD0306DC2B0CDA3C7DB261C19FFA35CA8EE24309E* L_0;
		L_0 = SslStream_get_Impl_m1356F55D556B6CBE922FB053F79BC930FCADF057(__this, NULL);
		ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* L_1 = ___0_buffer;
		int32_t L_2 = ___1_offset;
		int32_t L_3 = ___2_count;
		CancellationToken_t51142D9C6D7C02D314DA34A6A7988C528992FFED L_4 = ___3_cancellationToken;
		NullCheck(L_0);
		Task_1_t4C228DE57804012969575431CFF12D57C875552D* L_5;
		L_5 = VirtualFuncInvoker4< Task_1_t4C228DE57804012969575431CFF12D57C875552D*, ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031*, int32_t, int32_t, CancellationToken_t51142D9C6D7C02D314DA34A6A7988C528992FFED >::Invoke(18, L_0, L_1, L_2, L_3, L_4);
		return L_5;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Task_t751C4CC3ECD055BABA8A0B6A5DFBB4283DCA8572* SslStream_WriteAsync_m8211FC8F3FF4552E897516BE6CD70923848B89C3 (SslStream_t19A079881850F9CAD7BAA6FB625BBC4647ED5A27* __this, ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* ___0_buffer, int32_t ___1_offset, int32_t ___2_count, CancellationToken_t51142D9C6D7C02D314DA34A6A7988C528992FFED ___3_cancellationToken, const RuntimeMethod* method) 
{
	{
		MobileAuthenticatedStream_tD0306DC2B0CDA3C7DB261C19FFA35CA8EE24309E* L_0;
		L_0 = SslStream_get_Impl_m1356F55D556B6CBE922FB053F79BC930FCADF057(__this, NULL);
		ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* L_1 = ___0_buffer;
		int32_t L_2 = ___1_offset;
		int32_t L_3 = ___2_count;
		CancellationToken_t51142D9C6D7C02D314DA34A6A7988C528992FFED L_4 = ___3_cancellationToken;
		NullCheck(L_0);
		Task_t751C4CC3ECD055BABA8A0B6A5DFBB4283DCA8572* L_5;
		L_5 = VirtualFuncInvoker4< Task_t751C4CC3ECD055BABA8A0B6A5DFBB4283DCA8572*, ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031*, int32_t, int32_t, CancellationToken_t51142D9C6D7C02D314DA34A6A7988C528992FFED >::Invoke(22, L_0, L_1, L_2, L_3, L_4);
		return L_5;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* SslStream_BeginRead_m716BC242891C530ECB08DA4989AE27EEBAC71C98 (SslStream_t19A079881850F9CAD7BAA6FB625BBC4647ED5A27* __this, ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* ___0_buffer, int32_t ___1_offset, int32_t ___2_count, AsyncCallback_t7FEF460CBDCFB9C5FA2EF776984778B9A4145F4C* ___3_callback, RuntimeObject* ___4_state, const RuntimeMethod* method) 
{
	{
		MobileAuthenticatedStream_tD0306DC2B0CDA3C7DB261C19FFA35CA8EE24309E* L_0;
		L_0 = SslStream_get_Impl_m1356F55D556B6CBE922FB053F79BC930FCADF057(__this, NULL);
		ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* L_1 = ___0_buffer;
		int32_t L_2 = ___1_offset;
		int32_t L_3 = ___2_count;
		NullCheck(L_0);
		Task_1_t4C228DE57804012969575431CFF12D57C875552D* L_4;
		L_4 = Stream_ReadAsync_m734E5C146A1217C9E8FEC56ABDBD2AC33F5F8F87(L_0, L_1, L_2, L_3, NULL);
		AsyncCallback_t7FEF460CBDCFB9C5FA2EF776984778B9A4145F4C* L_5 = ___3_callback;
		RuntimeObject* L_6 = ___4_state;
		RuntimeObject* L_7;
		L_7 = TaskToApm_Begin_mA26D2A4F6EE0582B181AFD83B913916909D02777(L_4, L_5, L_6, NULL);
		return L_7;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t SslStream_EndRead_m881FE67FB5376508CBB50A96ED65BC672FB3A7A0 (SslStream_t19A079881850F9CAD7BAA6FB625BBC4647ED5A27* __this, RuntimeObject* ___0_asyncResult, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&TaskToApm_End_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mCA309E122256972A27C74FC814F47AD13AF623ED_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		RuntimeObject* L_0 = ___0_asyncResult;
		int32_t L_1;
		L_1 = TaskToApm_End_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mCA309E122256972A27C74FC814F47AD13AF623ED(L_0, TaskToApm_End_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mCA309E122256972A27C74FC814F47AD13AF623ED_RuntimeMethod_var);
		return L_1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* SslStream_BeginWrite_m66F3B2CDAAE0E1FD662552A8D1ECAC889F2AD2D0 (SslStream_t19A079881850F9CAD7BAA6FB625BBC4647ED5A27* __this, ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* ___0_buffer, int32_t ___1_offset, int32_t ___2_count, AsyncCallback_t7FEF460CBDCFB9C5FA2EF776984778B9A4145F4C* ___3_callback, RuntimeObject* ___4_state, const RuntimeMethod* method) 
{
	{
		MobileAuthenticatedStream_tD0306DC2B0CDA3C7DB261C19FFA35CA8EE24309E* L_0;
		L_0 = SslStream_get_Impl_m1356F55D556B6CBE922FB053F79BC930FCADF057(__this, NULL);
		ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* L_1 = ___0_buffer;
		int32_t L_2 = ___1_offset;
		int32_t L_3 = ___2_count;
		NullCheck(L_0);
		Task_t751C4CC3ECD055BABA8A0B6A5DFBB4283DCA8572* L_4;
		L_4 = Stream_WriteAsync_m51D91C94481BB32FE1A876A789C9705F433B133D(L_0, L_1, L_2, L_3, NULL);
		AsyncCallback_t7FEF460CBDCFB9C5FA2EF776984778B9A4145F4C* L_5 = ___3_callback;
		RuntimeObject* L_6 = ___4_state;
		RuntimeObject* L_7;
		L_7 = TaskToApm_Begin_mA26D2A4F6EE0582B181AFD83B913916909D02777(L_4, L_5, L_6, NULL);
		return L_7;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SslStream_EndWrite_m78C915054198C80ECEE67CBB07D782E10686BA61 (SslStream_t19A079881850F9CAD7BAA6FB625BBC4647ED5A27* __this, RuntimeObject* ___0_asyncResult, const RuntimeMethod* method) 
{
	{
		RuntimeObject* L_0 = ___0_asyncResult;
		TaskToApm_End_mDFB29EEE501409834D464F4C249A535723B7E6ED(L_0, NULL);
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR uint32_t U3CPrivateImplementationDetailsU3E_ComputeStringHash_m171C269D828658C44041FA68B6DE8CA290ED517F (String_t* ___0_s, const RuntimeMethod* method) 
{
	uint32_t V_0 = 0;
	int32_t V_1 = 0;
	{
		String_t* L_0 = ___0_s;
		if (!L_0)
		{
			goto IL_002a;
		}
	}
	{
		V_0 = ((int32_t)-2128831035);
		V_1 = 0;
		goto IL_0021;
	}

IL_000d:
	{
		String_t* L_1 = ___0_s;
		int32_t L_2 = V_1;
		NullCheck(L_1);
		Il2CppChar L_3;
		L_3 = String_get_Chars_mC49DF0CD2D3BE7BE97B3AD9C995BE3094F8E36D3(L_1, L_2, NULL);
		uint32_t L_4 = V_0;
		V_0 = ((int32_t)il2cpp_codegen_multiply(((int32_t)((int32_t)L_3^(int32_t)L_4)), ((int32_t)16777619)));
		int32_t L_5 = V_1;
		V_1 = ((int32_t)il2cpp_codegen_add(L_5, 1));
	}

IL_0021:
	{
		int32_t L_6 = V_1;
		String_t* L_7 = ___0_s;
		NullCheck(L_7);
		int32_t L_8;
		L_8 = String_get_Length_m42625D67623FA5CC7A44D47425CE86FB946542D2_inline(L_7, NULL);
		if ((((int32_t)L_6) < ((int32_t)L_8)))
		{
			goto IL_000d;
		}
	}

IL_002a:
	{
		uint32_t L_9 = V_0;
		return L_9;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void BypassElementCollection__ctor_m344EC8CD2EACA36E1213C5914037BB133E87FE54 (BypassElementCollection_t15465BC07F8DC8BABA6953182904AF762D638DBE* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&BypassElementCollection__ctor_m344EC8CD2EACA36E1213C5914037BB133E87FE54_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_raise_profile_exception(BypassElementCollection__ctor_m344EC8CD2EACA36E1213C5914037BB133E87FE54_RuntimeMethod_var);
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ConnectionManagementElementCollection__ctor_m4698342A715032A762FA04B2745BFCA46263BCB3 (ConnectionManagementElementCollection_tEEB0BEE8289364CCA1D76DDA3F862429612DE0F1* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ConnectionManagementElementCollection__ctor_m4698342A715032A762FA04B2745BFCA46263BCB3_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_raise_profile_exception(ConnectionManagementElementCollection__ctor_m4698342A715032A762FA04B2745BFCA46263BCB3_RuntimeMethod_var);
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ConnectionManagementSection__ctor_m2FA1D29F887BD6498FBEF692BBC0AA727E38E802 (ConnectionManagementSection_t8F4D45BE147F24F54F2124CC800B4594031F41C2* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ConnectionManagementSection__ctor_m2FA1D29F887BD6498FBEF692BBC0AA727E38E802_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_raise_profile_exception(ConnectionManagementSection__ctor_m2FA1D29F887BD6498FBEF692BBC0AA727E38E802_RuntimeMethod_var);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ConfigurationPropertyCollection_t1DEB95D3283BB11A46B862E9D13710ED698B6C93* ConnectionManagementSection_get_Properties_mFCE0F9418E9E8CD2E1301815CC4D548261F58E95 (ConnectionManagementSection_t8F4D45BE147F24F54F2124CC800B4594031F41C2* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ConnectionManagementSection_get_Properties_mFCE0F9418E9E8CD2E1301815CC4D548261F58E95_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_raise_profile_exception(ConnectionManagementSection_get_Properties_mFCE0F9418E9E8CD2E1301815CC4D548261F58E95_RuntimeMethod_var);
		return (ConfigurationPropertyCollection_t1DEB95D3283BB11A46B862E9D13710ED698B6C93*)NULL;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void DefaultProxySection__ctor_m962B68CEFCDE7DFB7646EF16297CA4BE0097284D (DefaultProxySection_tC0247A534955DC9D887B03AA4EFB1C1E01AD1E34* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&DefaultProxySection__ctor_m962B68CEFCDE7DFB7646EF16297CA4BE0097284D_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_raise_profile_exception(DefaultProxySection__ctor_m962B68CEFCDE7DFB7646EF16297CA4BE0097284D_RuntimeMethod_var);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ConfigurationPropertyCollection_t1DEB95D3283BB11A46B862E9D13710ED698B6C93* DefaultProxySection_get_Properties_m5186F1A2734EB96F065C7BC14A215D17F8325418 (DefaultProxySection_tC0247A534955DC9D887B03AA4EFB1C1E01AD1E34* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&DefaultProxySection_get_Properties_m5186F1A2734EB96F065C7BC14A215D17F8325418_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_raise_profile_exception(DefaultProxySection_get_Properties_m5186F1A2734EB96F065C7BC14A215D17F8325418_RuntimeMethod_var);
		return (ConfigurationPropertyCollection_t1DEB95D3283BB11A46B862E9D13710ED698B6C93*)NULL;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void DefaultProxySection_Reset_m0B9A1155840E7C55FC24AEDB23FB17A2815EEB77 (DefaultProxySection_tC0247A534955DC9D887B03AA4EFB1C1E01AD1E34* __this, ConfigurationElement_tAE3EE71C256825472831FFBB7F491275DFAF089E* ___0_parentElement, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&DefaultProxySection_Reset_m0B9A1155840E7C55FC24AEDB23FB17A2815EEB77_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_raise_profile_exception(DefaultProxySection_Reset_m0B9A1155840E7C55FC24AEDB23FB17A2815EEB77_RuntimeMethod_var);
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ProxyElement__ctor_mC1BDF426F97F4F84CB5A34B423F59EC8748EBD70 (ProxyElement_t4D277820F9C77333BE8CC6498DE7E06C36868C65* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ProxyElement__ctor_mC1BDF426F97F4F84CB5A34B423F59EC8748EBD70_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_raise_profile_exception(ProxyElement__ctor_mC1BDF426F97F4F84CB5A34B423F59EC8748EBD70_RuntimeMethod_var);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ConfigurationPropertyCollection_t1DEB95D3283BB11A46B862E9D13710ED698B6C93* ProxyElement_get_Properties_mBFE6E283FE7E9329B586ACAADC8F2E047C7ABB2A (ProxyElement_t4D277820F9C77333BE8CC6498DE7E06C36868C65* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ProxyElement_get_Properties_mBFE6E283FE7E9329B586ACAADC8F2E047C7ABB2A_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_raise_profile_exception(ProxyElement_get_Properties_mBFE6E283FE7E9329B586ACAADC8F2E047C7ABB2A_RuntimeMethod_var);
		return (ConfigurationPropertyCollection_t1DEB95D3283BB11A46B862E9D13710ED698B6C93*)NULL;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void HttpWebRequestElement__ctor_mB9DDAC7FF431D4AF4CAFCBC96E1A3572A1F83717 (HttpWebRequestElement_t8D2FEEC531D1FBE1B6E3D1077350A1EBDCB77DC3* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&HttpWebRequestElement__ctor_mB9DDAC7FF431D4AF4CAFCBC96E1A3572A1F83717_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_raise_profile_exception(HttpWebRequestElement__ctor_mB9DDAC7FF431D4AF4CAFCBC96E1A3572A1F83717_RuntimeMethod_var);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ConfigurationPropertyCollection_t1DEB95D3283BB11A46B862E9D13710ED698B6C93* HttpWebRequestElement_get_Properties_mAA0F11EA73B5397C50809EAC679F2C564F9EC7CA (HttpWebRequestElement_t8D2FEEC531D1FBE1B6E3D1077350A1EBDCB77DC3* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&HttpWebRequestElement_get_Properties_mAA0F11EA73B5397C50809EAC679F2C564F9EC7CA_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_raise_profile_exception(HttpWebRequestElement_get_Properties_mAA0F11EA73B5397C50809EAC679F2C564F9EC7CA_RuntimeMethod_var);
		return (ConfigurationPropertyCollection_t1DEB95D3283BB11A46B862E9D13710ED698B6C93*)NULL;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Ipv6Element__ctor_m869AC261E0994A8B3D83CEE25C6CB99E6A5EA4C1 (Ipv6Element_t480208D0E23E612DC1FD1CE19D9FB491F0F66932* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Ipv6Element__ctor_m869AC261E0994A8B3D83CEE25C6CB99E6A5EA4C1_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_raise_profile_exception(Ipv6Element__ctor_m869AC261E0994A8B3D83CEE25C6CB99E6A5EA4C1_RuntimeMethod_var);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ConfigurationPropertyCollection_t1DEB95D3283BB11A46B862E9D13710ED698B6C93* Ipv6Element_get_Properties_m382D5B5B098EFF19C2A4AC1889CD1C8EAB918322 (Ipv6Element_t480208D0E23E612DC1FD1CE19D9FB491F0F66932* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Ipv6Element_get_Properties_m382D5B5B098EFF19C2A4AC1889CD1C8EAB918322_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_raise_profile_exception(Ipv6Element_get_Properties_m382D5B5B098EFF19C2A4AC1889CD1C8EAB918322_RuntimeMethod_var);
		return (ConfigurationPropertyCollection_t1DEB95D3283BB11A46B862E9D13710ED698B6C93*)NULL;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void NetSectionGroup__ctor_mCD2F0DC2B9F0C6C346BC91BD8750C7111A9528AC (NetSectionGroup_tA83DCAF89773087D0E921B92F67441132B71D52F* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&NetSectionGroup__ctor_mCD2F0DC2B9F0C6C346BC91BD8750C7111A9528AC_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_raise_profile_exception(NetSectionGroup__ctor_mCD2F0DC2B9F0C6C346BC91BD8750C7111A9528AC_RuntimeMethod_var);
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SettingsSection__ctor_mBD94E7E098221DC09775A3AE1713930F6820D242 (SettingsSection_tC931BA69B22D5E0ED07698F02778482987319A12* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SettingsSection__ctor_mBD94E7E098221DC09775A3AE1713930F6820D242_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_raise_profile_exception(SettingsSection__ctor_mBD94E7E098221DC09775A3AE1713930F6820D242_RuntimeMethod_var);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ConfigurationPropertyCollection_t1DEB95D3283BB11A46B862E9D13710ED698B6C93* SettingsSection_get_Properties_mA774C9C884F4AD5DEC2BBE31DF7E35CAC71EBF8F (SettingsSection_tC931BA69B22D5E0ED07698F02778482987319A12* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SettingsSection_get_Properties_mA774C9C884F4AD5DEC2BBE31DF7E35CAC71EBF8F_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_raise_profile_exception(SettingsSection_get_Properties_mA774C9C884F4AD5DEC2BBE31DF7E35CAC71EBF8F_RuntimeMethod_var);
		return (ConfigurationPropertyCollection_t1DEB95D3283BB11A46B862E9D13710ED698B6C93*)NULL;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void PerformanceCountersElement__ctor_m9537195CBC7DC607F7253D06E7B6B1090456E68F (PerformanceCountersElement_tCB62C8CA7C218E35653DC5BB690A5FFD6D608B30* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&PerformanceCountersElement__ctor_m9537195CBC7DC607F7253D06E7B6B1090456E68F_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_raise_profile_exception(PerformanceCountersElement__ctor_m9537195CBC7DC607F7253D06E7B6B1090456E68F_RuntimeMethod_var);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ConfigurationPropertyCollection_t1DEB95D3283BB11A46B862E9D13710ED698B6C93* PerformanceCountersElement_get_Properties_m7C16FBC3E915E8384FB4409209628737B1537C9B (PerformanceCountersElement_tCB62C8CA7C218E35653DC5BB690A5FFD6D608B30* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&PerformanceCountersElement_get_Properties_m7C16FBC3E915E8384FB4409209628737B1537C9B_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_raise_profile_exception(PerformanceCountersElement_get_Properties_m7C16FBC3E915E8384FB4409209628737B1537C9B_RuntimeMethod_var);
		return (ConfigurationPropertyCollection_t1DEB95D3283BB11A46B862E9D13710ED698B6C93*)NULL;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ServicePointManagerElement__ctor_m0F1CF21589CA283DA734A472930A9665698B2D36 (ServicePointManagerElement_t98ED8374270537E77E57D014ECEE8B1A3A8EEFF2* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ServicePointManagerElement__ctor_m0F1CF21589CA283DA734A472930A9665698B2D36_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_raise_profile_exception(ServicePointManagerElement__ctor_m0F1CF21589CA283DA734A472930A9665698B2D36_RuntimeMethod_var);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ConfigurationPropertyCollection_t1DEB95D3283BB11A46B862E9D13710ED698B6C93* ServicePointManagerElement_get_Properties_m70469523AAD1A2C2E9D4F90DDA1BA619FCE68B58 (ServicePointManagerElement_t98ED8374270537E77E57D014ECEE8B1A3A8EEFF2* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ServicePointManagerElement_get_Properties_m70469523AAD1A2C2E9D4F90DDA1BA619FCE68B58_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_raise_profile_exception(ServicePointManagerElement_get_Properties_m70469523AAD1A2C2E9D4F90DDA1BA619FCE68B58_RuntimeMethod_var);
		return (ConfigurationPropertyCollection_t1DEB95D3283BB11A46B862E9D13710ED698B6C93*)NULL;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SocketElement__ctor_m4FB4C851F1BB20CED74C206EFB1BA9DDFBE024D2 (SocketElement_tDC83FCE3C280D8D590972F989292C30E2B7F6CC6* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SocketElement__ctor_m4FB4C851F1BB20CED74C206EFB1BA9DDFBE024D2_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_raise_profile_exception(SocketElement__ctor_m4FB4C851F1BB20CED74C206EFB1BA9DDFBE024D2_RuntimeMethod_var);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ConfigurationPropertyCollection_t1DEB95D3283BB11A46B862E9D13710ED698B6C93* SocketElement_get_Properties_m8AF94DEEDA0EB4092F59FBDC361592E1B9EBDEC3 (SocketElement_tDC83FCE3C280D8D590972F989292C30E2B7F6CC6* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SocketElement_get_Properties_m8AF94DEEDA0EB4092F59FBDC361592E1B9EBDEC3_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_raise_profile_exception(SocketElement_get_Properties_m8AF94DEEDA0EB4092F59FBDC361592E1B9EBDEC3_RuntimeMethod_var);
		return (ConfigurationPropertyCollection_t1DEB95D3283BB11A46B862E9D13710ED698B6C93*)NULL;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WebProxyScriptElement__ctor_mB613D71DE569317E40726CD8DB9A6ABA0A302821 (WebProxyScriptElement_t0C0E2338F7F42D5D386F5AE68FF7F870FA6E7167* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&WebProxyScriptElement__ctor_mB613D71DE569317E40726CD8DB9A6ABA0A302821_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_raise_profile_exception(WebProxyScriptElement__ctor_mB613D71DE569317E40726CD8DB9A6ABA0A302821_RuntimeMethod_var);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ConfigurationPropertyCollection_t1DEB95D3283BB11A46B862E9D13710ED698B6C93* WebProxyScriptElement_get_Properties_mF5188225C0C1D64BC136465BB4B0E962FFE9240B (WebProxyScriptElement_t0C0E2338F7F42D5D386F5AE68FF7F870FA6E7167* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&WebProxyScriptElement_get_Properties_mF5188225C0C1D64BC136465BB4B0E962FFE9240B_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_raise_profile_exception(WebProxyScriptElement_get_Properties_mF5188225C0C1D64BC136465BB4B0E962FFE9240B_RuntimeMethod_var);
		return (ConfigurationPropertyCollection_t1DEB95D3283BB11A46B862E9D13710ED698B6C93*)NULL;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WebRequestModulesSection__ctor_mF25CD80A5111A4F87B7FBF7663A3442470B97C0E (WebRequestModulesSection_t4099F9890D7851314C5528C577E4C51B36C97D00* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&WebRequestModulesSection__ctor_mF25CD80A5111A4F87B7FBF7663A3442470B97C0E_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_raise_profile_exception(WebRequestModulesSection__ctor_mF25CD80A5111A4F87B7FBF7663A3442470B97C0E_RuntimeMethod_var);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ConfigurationPropertyCollection_t1DEB95D3283BB11A46B862E9D13710ED698B6C93* WebRequestModulesSection_get_Properties_mBDBDED569E677717FB617CD36B94AE34BF4208F6 (WebRequestModulesSection_t4099F9890D7851314C5528C577E4C51B36C97D00* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&WebRequestModulesSection_get_Properties_mBDBDED569E677717FB617CD36B94AE34BF4208F6_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_raise_profile_exception(WebRequestModulesSection_get_Properties_mBDBDED569E677717FB617CD36B94AE34BF4208F6_RuntimeMethod_var);
		return (ConfigurationPropertyCollection_t1DEB95D3283BB11A46B862E9D13710ED698B6C93*)NULL;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void WebRequestModuleElementCollection__ctor_m62C156D78336B2E37EA6F10D477FDEAE4161F99E (WebRequestModuleElementCollection_t5979A19A48B822831C1D351EB2AA25058C7F7463* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&WebRequestModuleElementCollection__ctor_m62C156D78336B2E37EA6F10D477FDEAE4161F99E_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_raise_profile_exception(WebRequestModuleElementCollection__ctor_m62C156D78336B2E37EA6F10D477FDEAE4161F99E_RuntimeMethod_var);
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void DiagnosticsConfigurationHandler__ctor_mDFB0000E6218840F66D21BB6C9BF3F647CCBC5A3 (DiagnosticsConfigurationHandler_t0C4B611E2C6EA17B288C43070FAF65F96DCA96E7* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&DiagnosticsConfigurationHandler__ctor_mDFB0000E6218840F66D21BB6C9BF3F647CCBC5A3_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_raise_profile_exception(DiagnosticsConfigurationHandler__ctor_mDFB0000E6218840F66D21BB6C9BF3F647CCBC5A3_RuntimeMethod_var);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* DiagnosticsConfigurationHandler_Create_mF21B1CC68B87DE078690B2B493FCDC5F728CBAAB (DiagnosticsConfigurationHandler_t0C4B611E2C6EA17B288C43070FAF65F96DCA96E7* __this, RuntimeObject* ___0_parent, RuntimeObject* ___1_configContext, XmlNode_t3180B9B3D5C36CD58F5327D9F13458E3B3F030AF* ___2_section, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&DiagnosticsConfigurationHandler_Create_mF21B1CC68B87DE078690B2B493FCDC5F728CBAAB_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_raise_profile_exception(DiagnosticsConfigurationHandler_Create_mF21B1CC68B87DE078690B2B493FCDC5F728CBAAB_RuntimeMethod_var);
		return NULL;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ThrowStub_ThrowNotSupportedException_m14168B43936EF78B7B8AC08BD815254DAA1D9FA2 (const RuntimeMethod* method) 
{
	{
		PlatformNotSupportedException_tD2BD7EB9278518AA5FE8AE75AD5D0D4298A4631A* L_0 = (PlatformNotSupportedException_tD2BD7EB9278518AA5FE8AE75AD5D0D4298A4631A*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&PlatformNotSupportedException_tD2BD7EB9278518AA5FE8AE75AD5D0D4298A4631A_il2cpp_TypeInfo_var)));
		PlatformNotSupportedException__ctor_mD5DBE8E9A6FF4B75EF02671029C6D67A51EAFBD1(L_0, NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_0, ((RuntimeMethod*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ThrowStub_ThrowNotSupportedException_m14168B43936EF78B7B8AC08BD815254DAA1D9FA2_RuntimeMethod_var)));
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Exception_set_HResult_m010A171183E240EBF4F7611E924B533FAB2E471F_inline (Exception_t* __this, int32_t ___0_value, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = ___0_value;
		__this->____HResult = L_0;
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR String_t* MobileAuthenticatedStream_get_TargetHost_m572DA701CB3CE3FA4006D5745F9A3B9A52739CAB_inline (MobileAuthenticatedStream_tD0306DC2B0CDA3C7DB261C19FFA35CA8EE24309E* __this, const RuntimeMethod* method) 
{
	{
		String_t* L_0 = __this->___U3CTargetHostU3Ek__BackingField;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* AuthenticatedStream_get_InnerStream_m5BDA4857898A90C696DAD90FD61C536FC3EB10C7_inline (AuthenticatedStream_t8DCF41E151F705E2494FC7836F5E2EF7C539FA39* __this, const RuntimeMethod* method) 
{
	{
		Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* L_0 = __this->____InnerStream;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t String_get_Length_m42625D67623FA5CC7A44D47425CE86FB946542D2_inline (String_t* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____stringLength;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR ConfiguredTaskAwaiter_t8FC486401D1951F2426478F2050EE290197B6E80 ConfiguredTaskAwaitable_1_GetAwaiter_m7A77B1981FEC19CC7E1570EDC3F16AC5C14B4439_gshared_inline (ConfiguredTaskAwaitable_1_t5186C81524388C1718E9AC37792D8771A443417A* __this, const RuntimeMethod* method) 
{
	{
		ConfiguredTaskAwaiter_t8FC486401D1951F2426478F2050EE290197B6E80 L_0 = __this->___m_configuredTaskAwaiter;
		return L_0;
	}
}
