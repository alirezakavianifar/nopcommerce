
<html class="loading light-layout " data-layout="light-layout" lang="fa" data-textdirection="rtl">
<!-- BEGIN: Head-->
<head>
    <meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <meta name="viewport"
          content="width=device-width,initial-scale=1.0,user-scalable=1,maximum-scale=2.0 ,minimal-ui,interactive-widget=resizes-content">
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <meta property="og:site_name" content="فریلنسر باشید یا پروژه برون‌سپاری کنید - پارس‌کدرز">
    <link rel="manifest" href="/manifest.json">

    <meta property="og:image" itemprop="image"
          content="/assets/landing/img/white_logo.jpg">

    

    
    
    
    
    
    
    

    
    <link rel="shortcut icon" type="image/x-icon" href="/favicon.ico">
                            
                
        
        
        
        
        <link rel="stylesheet" type="text/css"
              href="/vuexy/app-assets/css/plugins/extensions/ext-component-sweet-alerts.css">

    <link rel="stylesheet" type="text/css"
              href="/vuexy/app-assets/css/plugins/extensions/ext-component-toastr.min.css">
        

    
        
        
        
        
        
        
        
        

    
        <link rel="stylesheet" type="text/css"
              href="/vuexy/app-assets/css-rtl/core/menu/menu-types/horizontal-menu.css">
                                

    
        
        
        

    
        
        

    

    

    
    
    
    
        let message,message2, domain;
        const expectedTime = 15000;
                message = 'به نظر می‌رسد در حال حاضر سرعت سایت پارس‌کدرز برای کاربران خارج از ایران پایین است. با استفاده از آدرس زیر می توانید تجربه بهتری داشته باشید:';
        message2 = 'آدرس برای کاربران خارج از ایران: '
        domain = 'parscoders.de'
                let t = setTimeout(function () {
            const alert = document.createElement("div");
            // alert.style = 'position: fixed; z-index:9999; top: 50px; right: 50px;width:300px;height:50px;background-color:#f0f0f0;border:1px solid red';
            alert.id='lowSpeedAlert'
            alert.style='position:fixed; top:0; left:0; right:0; background-color: #fff0f0; color: black; margin:25px;border-radius:5px; padding: 15px; text-align: center; z-index: 9999;border:1px solid orange;';
            alert.innerHTML = '<span style="float:left; cursor:pointer;color:orange;margin-top: -10px;margin-left: -5px;font-weight: bolder" onclick="document.getElementById(\'lowSpeedAlert\').style.display=\'none\'">X</span>'+
                message+''+
                message2 +
                '<span style="color: blue;text-decoration: underline;cursor: pointer;font-family:Monospace" onclick="window.location=\'https://'+domain+'\'">'+domain+'</span>'+
                '

<!-- END: Head-->

<!-- BEGIN: Body-->

<body class="horizontal-layout horizontal-menu  navbar-floating footer-static  " data-open="hover"
      data-menu="horizontal-menu" data-col="">

<nav class="header-navbar navbar-expand-lg navbar navbar-fixed align-items-center navbar-shadow navbar-brand-center"
     data-nav="brand-center">
    <div class="navbar-header d-xl-block d-none">
        <ul class="nav navbar-nav">
            <li class="nav-item"><a class="navbar-brand" href="/">
                    <span class="brand-logo"></span>
                    <span class="brand-text mb-0">پارس‌کدرز</span>
                </a></li>
        </ul>
    </div>
    <div class="navbar-container d-flex content">
        <div class="bookmark-wrapper d-flex align-items-center">
            <ul class="nav navbar-nav d-xl-none">
                <li class="nav-item">
                    <a class="nav-link menu-toggle" href="#">
                        <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none"
                             stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"
                             class="feather feather-menu ficon">
                            <line x1="3" y1="12" x2="21" y2="12"></line>
                            <line x1="3" y1="6" x2="21" y2="6"></line>
                            <line x1="3" y1="18" x2="21" y2="18"></line>
                        </svg>
                    </a>
                </li>
                <li class="nav-item dropdown ms-75 d-block d-sm-none">
                    <a
                            id="menu_create_project_btn"
                            data-bs-toggle="dropdown" aria-expanded="true"
                            class="btn btn-warning px-25 py-25 nav-link dropdown-toggle" href="#" type="button">
                        <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none"
                             stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"
                             class="feather feather-plus ficon text-white">
                            <line x1="12" y1="5" x2="12" y2="19"></line>
                            <line x1="5" y1="12" x2="19" y2="12"></line>
                        </svg>
                    </a>
                    <div class="dropdown-menu max-width-400 dropdown-menu-media ms-4 pe-2"
                         aria-labelledby="menu_create_project_btn"
                         data-popper-placement="bottom-start">
                        <a class="dropdown-item" href="/anonymous-project/create">
                            <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24"
                                 fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"
                                 stroke-linejoin="round" class="feather feather-plus">
                                <line x1="12" y1="5" x2="12" y2="19"></line>
                                <line x1="5" y1="12" x2="19" y2="12"></line>
                            </svg>
                                                            سفارش رایگان پروژه
                                                    </a>
                    </div>
                </li>
            </ul>
                            <ul class="nav navbar-nav bookmark-icons">
                                        <li class="nav-item d-none d-sm-block">
                        <a class="nav-link user--chats-toggle" target="_blank"
                           aria-label="مکالمات"
                           id="userChatsToggle"
                           data-bs-toggle="offcanvas"
                           data-bs-target="#chatsOffcanvas"
                           href="/chat"
                                >
                            <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24"
                                 fill="none" aria-label="آیکن مکالمات"
                                 stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"
                                 class="feather feather-message-square ficon">
                                <path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"></path>
                            </svg>
                        </a>
                    </li>
                                                        </ul>
                <ul class="nav navbar-nav">
                    <li class="nav-item d-none d-sm-block">
                        <a
                                href="/bookmarks"
                                data-bs-toggle="tooltip"
                                title="ذخیره‌شده‌ها"
                                aria-label="ذخیره‌ شده‌ها"
                                class="nav-link bookmark-star">
                            <i class="far fa-star text-warning font-medium-2"></i>
                        </a>
                    </li>
                </ul>
                        <ul class="nav navbar-nav">
                <li class="nav-item d-none d-sm-block">
                    <a class="btn btn-sm py-75 ms-1 btn-warning" href="/anonymous-project/create">
                        <i class="fal fa-plus"></i>
                                                    سفارش رایگان پروژه
                                            </a>
                </li>
            </ul>
        </div>
        <ul class="nav navbar-nav align-items-center ms-auto">
                        <li class="nav-item d-block d-sm-none">
                <a
                        href="/bookmarks"
                        aria-label="ذخیره‌ شده‌ها"
                        class="nav-link bookmark-star">
                    <i class="far fa-star text-warning font-medium-3"></i>
                </a>
            </li>
            <li class="nav-item d-block d-sm-none">
                <a class="nav-link" href="/chat">
                    <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24"
                         fill="none" aria-label="آیکن مکالمات"
                         stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"
                         class="feather feather-message-square ficon">
                        <path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"></path>
                    </svg>
                </a>
            </li>
                        <li class="nav-item d-none d-lg-block">
                <a id="change-layout" class="nav-link" tabindex="0" aria-label="تنظیم حالت تیره صفحه">
                                            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none"
                             stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"
                             aria-label="آیکن ماه"
                             class="feather feather-moon ficon">
                            <path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z"></path>
                        </svg>
                                    </a>
            </li>
            <li class="nav-item dropdown nav-search">
                <a
                        data-bs-toggle="dropdown" id="search_link" tabindex="0"
                        class="nav-link nav-link-search" aria-label="جستجو">
                    <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none"
                         stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"
                         aria-labelledby="search_link"
                         class="feather feather-search ficon">
                        <circle cx="11" cy="11" r="8"></circle>
                        <line x1="21" y1="21" x2="16.65" y2="16.65"></line>
                    </svg>
                </a>

*
*

*

*

*

*

*

*

*

*

*

*

*

*

*

*

*

*

*

*

*

*

*

*

*

*

*

        <a
                id="show_all_notifications"
                href="/notification/index?type=system"
                class="btn btn-primary w-100">    مشاهده            همه</li>

</ul>
                </li>
                                <li class="nav-item dropdown dropdown-user"><a class="nav-link dropdown-toggle dropdown-user-link"
                                                               id="dropdown-user" href="#" data-bs-toggle="dropdown"
                                                               aria-haspopup="true" aria-expanded="false">
                        <div class="user-nav d-sm-flex d-none"><span
                                    class="user-name fw-bolder mt-25 mt-xl-auto">علیرضا کاویانی فر</span><span
                                    class="user-status mt-50 mt-xl-auto">Programmer_AI_Expert</span></div>
                        <span class="avatar">
                            <img class="round"
                                 src="https://parscoders.com/avatar/tiny/413919_f71b209caa1832535a9c0e27ca24f982.webp"
                                 alt="avatar" height="40" width="40">
                            <span
                                    id="connectionIndicator"
                                    data-online-status="1" data-userid="413919"
                                    class="avatar-status-offline"></span>
                        </span>
                    </a>
                    <div class="dropdown-menu dropdown-menu-end w-auto" aria-labelledby="dropdown-user">
                                                                                                <a class="dropdown-item" href="/user-info/update">
                                                        <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24"
                                 fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"
                                 stroke-linejoin="round" class="feather feather-edit me-50">
                                <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path>
                                <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"></path>
                            </svg>
                            ویرایش اطلاعات/پروفایل
                        </a>
                        <a class="dropdown-item" href="/account/setting">
                            <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24"
                                 fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"
                                 stroke-linejoin="round" class="feather feather-settings me-50">
                                <circle cx="12" cy="12" r="3"></circle>
                                <path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 0 1 0 2.83 2 2 0 0 1-2.83 0l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-2 2 2 2 0 0 1-2-2v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 0 1-2.83 0 2 2 0 0 1 0-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1-2-2 2 2 0 0 1 2-2h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 0 1 0-2.83 2 2 0 0 1 2.83 0l.06.06a1.65 1.65 0 0 0 1.82.33H9a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 2-2 2 2 0 0 1 2 2v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 0 1 2.83 0 2 2 0 0 1 0 2.83l-.06.06a1.65 1.65 0 0 0-.33 1.82V9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 2 2 2 2 0 0 1-2 2h-.09a1.65 1.65 0 0 0-1.51 1z"></path>
                            </svg>
                            تنظیمات حساب کاربری
                        </a>
                        <a class="dropdown-item" href="/user-verification/">
                            <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24"
                                 fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"
                                 stroke-linejoin="round" class="feather feather-check-square me-50">
                                <polyline points="9 11 12 14 22 4"></polyline>
                                <path d="M21 12v7a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11"></path>
                            </svg>
                            تایید حساب کاربری
                        </a>
                        <div class="dropdown-divider"></div>
                        <a class="dropdown-item" href="/user/change-password">
                            <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24"
                                 fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"
                                 stroke-linejoin="round" class="feather feather-key me-50">
                                <path d="M21 2l-2 2m-7.61 7.61a5.5 5.5 0 1 1-7.778 7.778 5.5 5.5 0 0 1 7.777-7.777zm0 0L15.5 7.5m0 0l3 3L22 7l-3-3m-3.5 3.5L19 4"></path>
                            </svg>
                            تغییر کلمه عبور
                        </a>
                                                                        <div class="dropdown-divider"></div>
                        <a class="dropdown-item" href="/coupon">
                            <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24"
                                 fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"
                                 stroke-linejoin="round" class="feather feather-percent me-50">
                                <line x1="19" y1="5" x2="5" y2="19"></line>
                                <circle cx="6.5" cy="6.5" r="2.5"></circle>
                                <circle cx="17.5" cy="17.5" r="2.5"></circle>
                            </svg>
                            ثبت کد تخفیف
                        </a>
                        <div class="dropdown-divider"></div>
                        <a class="dropdown-item" href="/invite/">
                            <i class="far fa-users me-50"></i>
                            دعوت از دوستان
                        </a>
                                                    <div class="dropdown-divider"></div>
                            <a
                                    class="dropdown-item"
                                    data-bs-toggle="modal"
                                    href="#" tabindex="0"
                                    data-bs-target="#userExperienceModal">
                                <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24"
                                     fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"
                                     stroke-linejoin="round" class="feather feather-refresh-cw me-50">
                                    <polyline points="23 4 23 10 17 10"></polyline>
                                    <polyline points="1 20 1 14 7 14"></polyline>
                                    <path d="M3.51 9a9 9 0 0 1 14.85-3.36L23 10M1 14l4.64 4.36A9 9 0 0 0 20.49 15"></path>
                                </svg>
                                بازگشت به قالب قدیم
                            </a>
                                                <div class="dropdown-divider"></div>
                        <a class="dropdown-item" href="/logout">
                            <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24"
                                 fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"
                                 stroke-linejoin="round" class="feather feather-power me-50">
                                <path d="M18.36 6.64a9 9 0 1 1-12.73 0"></path>
                                <line x1="12" y1="2" x2="12" y2="12"></line>
                            </svg>
                            خروج
                        </a>
                    </div>
                </li>
                    </ul>
    </div>
</nav>
<ul class="main-search-list-defaultlist-other-list d-none">
    <li class="auto-suggestion justify-content-between"><a
                class="d-flex align-items-center justify-content-between w-100 py-50">
            <div class="d-flex justify-content-start"><span class="me-75" data-feather="alert-circle"></span><span>No results found.</span>
            </div>
        </a></li>
</ul>
<!-- END: Header-->
    <!-- BEGIN: Main Menu-->
<div class="horizontal-menu-wrapper">
    <div class="header-navbar navbar-expand-sm navbar navbar-horizontal floating-nav navbar-light navbar-shadow menu-border container-xxl"
         role="navigation" data-menu="menu-wrapper" data-menu-type="floating-nav">
        <div class="navbar-header">
            <ul class="nav navbar-nav flex-row">
                <li class="nav-item me-auto"><a class="navbar-brand"
                                                href="/">
                                                <span class="brand-text mb-0">پارس‌کدرز</span>
                    </a></li>
                <li class="nav-item nav-toggle">
                    <span class="nav-link modern-nav-toggle pe-0" data-bs-toggle="collapse">
                        <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none"
                             stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"
                             class="feather feather-x d-block d-xl-none text-primary toggle-icon font-medium-4">
                            <line x1="18" y1="6" x2="6" y2="18"></line>
                            <line x1="6" y1="6" x2="18" y2="18"></line>
                        </svg>
                    </span>
                </li>
            </ul>
        </div>
        <div class="shadow-bottom"></div>
        <!-- Horizontal menu content-->
        <div class="navbar-container main-menu-content d-none d-xl-flex horizontal--menu" data-menu="menu-container">
                                        <ul     id="main-menu-navigation" class="nav navbar-nav" data-menu="menu-navigation">

</div>
<!-- END: Main Menu-->

<!-- BEGIN: Content-->

                    <button type="button" class="btn-close" data-dismiss="modal" data-bs-dismiss="modal"
                            aria-label="Close"></button><span
                                aria-hidden="true">                    </button>**************************
