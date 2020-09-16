using System;
// ******************************************************************************
// * GX error file - v7.2.8
// * 2004-2010 (c) ARH Inc. (http://www.arhungary.hu)
// ******************************************************************************
// GX system error codes and messages </b>
// The GX is the base system for the majority of the Adaptive Recognition Hungary Inc. products.
// It is a collection of loadable modules and library functions and gives an easy to program
// interface to the hardware devices.
// This file contains the error codes and descriptions of the GX system.
// ******************************************************************************
public class GxErrorCode
{
	
	// System and GX-specific error codes.
	public const long GX_ENOERR = 0x0; // No Error
	public const long GX_ENOENT = 0x2; // Entry not found (ENOENT)
	public const long GX_ENOMEM = 0xC; // Memory allocation error (ENOMEM)
	public const long GX_EACCES = 0xD; // Permission denied (EACCES)
	public const long GX_EFAULT = 0xE; // Bad address or program error (EFAULT)
	public const long GX_EBUSY = 0x10; // Resource busy (EBUSY)
	public const long GX_EEXIST = 0x11; // File exists (EEXIST)
	public const long GX_ENODEV = 0x13; // No such device (ENODEV)
	public const long GX_EINVAL = 0x16; // Invalid parameter (EINVAL)
	public const long GX_ERANGE = 0x22; // Data out of range (ERANGE)
	public const long GX_EDATA = 0x3D; // No data available (Linux - ENODATA)
	public const long GX_ECOMM = 0x46; // Communication error on send (Linux - ECOMM)
	public const long GX_ETIMEDOUT = 0x6E; // Function timed out (Linux - ETIMEDOUT)
	
	// General error codes
	public const long GX_EOPEN = 0x1000; // File open error
	public const long GX_ECREAT = 0x1001; // File creation error
	public const long GX_EREAD = 0x1002; // File read error
	public const long GX_EWRITE = 0x1003; // File write error
	public const long GX_EFILE = 0x1004; // File content
	
	public const long GX_EINVIMG = 0x1010; // Invalid image
	public const long GX_EINVFUNC = 0x1011; // Invalid function
	
	public const long GX_EHWKEY = 0x1012; // Hardware key does not work properly
	public const long GX_EVERSION = 0x1013; // Invalid version
	public const long GX_EASSERT = 0x1014; // Assertion occurred
	
	public const long GX_EDISCON = 0x1015; // Device is disconnected
	public const long GX_EIMGPROC = 0x1016; // Image processing failed
	public const long GX_EAUTH = 0x1017; // Authenticity cannot be determined
	
	// Module error codes
	// GX_xxx = 0xmmmm8xxx    // mmmm => group code
	
	public const long GX_ENOMODULE = 0x8000; // The specified module not found (module: '%ls')
	
}
