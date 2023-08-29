# Release Notes
## Version 2.2.0
*	Added support for dynamic service registration via IDynamicServiceRegistrar (native) and IDynamicServiceConfigurer (compatibility).
*	Changed default behavior when a service cannot be resolved to return null rather than throw an exception.
	Added AlwaysRequireResolution flag to ServiceContainerOptions to control the behavior.
*	Added an exception when a scope key depends on a service with a lifetime other than Scoped.