class AdminMenu extends HTMLElement {
    constructor() {
        super();
    }
    connectedCallback() {
        this.innerHTML = '<ul class="navigation navigation-main" id="main-menu-navigation" data-menu="menu-navigation" data-icon-style="lines">'
            + '     <li class=" nav-item">'
            + '         <a href="/admin/index.htm"><img src="/admin/imgs/streamline-icon-bird-house_24.svg" /><span class="menu-title" data-i18n="">Dashboard</span></a>'
            + '     </li>'
            + '     <li class=" nav-item">'
            + '         <a href="/admin/agents.htm"><img src="/admin/imgs/streamline-icon-single-neutral-home_24.svg" /><span class="menu-title" data-i18n="">Agents &amp; services</span></a>'
            + '     </li>'
            + '     <li class=" nav-item">'
            + '         <a href="/admin/users.htm"><img src="/admin/imgs/streamline-icon-multiple-users-1_24.png" /><span class="menu-title" data-i18n="">Users &amp; contacts</span></a>'
            + '     </li>'
            + '     <li class=" nav-item">'
            + '         <a href="/admin/apps.htm"><img src="/admin/imgs/streamline-icon-app-window-module_24.png" /><span class="menu-title" data-i18n="">Apps &amp; extensions</span></a>'
            + '     </li>'
            + '     <li class=" nav-item">'
            + '         <a href="/admin/devices.htm"><img src="/admin/imgs/streamline-icon-smart-house-phone-connect_24.png" /><span class="menu-title" data-i18n="">Devices</span></a>'
            + '     </li>'
            + '     <li class=" nav-item">'
            + '         <a href="/admin/tokens.htm"><img src="/admin/imgs/streamline-icon-lock-hierarchy_24.png" /><span class="menu-title" data-i18n="">Connected accounts</span></a>'
            + '     </li>'
            + '     <li class=" nav-item">'
            + '         <a href="/admin/locations.htm"><img src="/admin/imgs/streamline-icon-real-estate-action-house-pin_24.png" /><span class="menu-title" data-i18n="">Locations &amp; Mesh</span></a>'
            + '     </li>'
            + ' </ul>';
    }
}
window.customElements.define('admin-menu', AdminMenu);
//# sourceMappingURL=admin.js.map