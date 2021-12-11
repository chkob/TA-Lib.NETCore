using System;

namespace TALib.NETCore.HighPerf
{
    public enum RetCode : ushort
    {
        Success,
        LibNotInitialize,
        BadParam,
        GroupNotFound,
        FuncNotFound,
        InvalidHandle,
        InvalidParamHolder,
        InvalidParamHolderType,
        InvalidParamFunction,
        InputNotAllInitialize,
        OutputNotAllInitialize,
        OutOfRangeStartIndex,
        OutOfRangeEndIndex,
        InvalidListType,
        BadObject,
        NotSupported,
        InternalError = 5000,
        UnknownErr = UInt16.MaxValue
    }
}
