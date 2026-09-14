import os
import shutil
import zipfile

def make_plugin_zip(source_dir, plugin_name, output_zip_path):
    temp_dir = os.path.join(os.environ.get("TEMP", "/tmp"), f"pkg_{plugin_name}")
    if os.path.exists(temp_dir):
        shutil.rmtree(temp_dir)
    
    plugin_target_dir = os.path.join(temp_dir, plugin_name)
    os.makedirs(plugin_target_dir, exist_ok=True)
    
    # Files to include
    target_files = [
        "plugin.json",
        f"Nop.Plugin.{plugin_name}.dll",
        f"Nop.Plugin.{plugin_name}.pdb",
        f"Nop.Plugin.{plugin_name}.deps.json"
    ]
    
    for f in target_files:
        src_file = os.path.join(source_dir, f)
        if os.path.exists(src_file):
            shutil.copy2(src_file, os.path.join(plugin_target_dir, f))
            print(f"[{plugin_name}] Copied: {f}")
        else:
            print(f"[{plugin_name}] Note: {f} not found, skipping.")
            
    # Copy Views folder
    views_src = os.path.join(source_dir, "Views")
    if os.path.exists(views_src):
        shutil.copytree(views_src, os.path.join(plugin_target_dir, "Views"))
        print(f"[{plugin_name}] Copied Views directory.")
        
    # Create zip
    if os.path.exists(output_zip_path):
        os.remove(output_zip_path)
        
    os.makedirs(os.path.dirname(output_zip_path), exist_ok=True)
    
    with zipfile.ZipFile(output_zip_path, 'w', zipfile.ZIP_DEFLATED) as zipf:
        for root, dirs, files in os.walk(temp_dir):
            for file in files:
                full_path = os.path.join(root, file)
                rel_path = os.path.relpath(full_path, temp_dir)
                zipf.write(full_path, rel_path)
                
    shutil.rmtree(temp_dir)
    size_kb = os.path.getsize(output_zip_path) / 1024
    print(f"Created: {output_zip_path} ({size_kb:.1f} KB)")


def make_combined_bundle(plugins, output_zip_path):
    temp_dir = os.path.join(os.environ.get("TEMP", "/tmp"), "pkg_combined")
    if os.path.exists(temp_dir):
        shutil.rmtree(temp_dir)
    os.makedirs(temp_dir, exist_ok=True)
    
    for source_dir, plugin_name in plugins:
        plugin_target_dir = os.path.join(temp_dir, plugin_name)
        os.makedirs(plugin_target_dir, exist_ok=True)
        
        target_files = [
            "plugin.json",
            f"Nop.Plugin.{plugin_name}.dll",
            f"Nop.Plugin.{plugin_name}.pdb",
            f"Nop.Plugin.{plugin_name}.deps.json"
        ]
        
        for f in target_files:
            src_file = os.path.join(source_dir, f)
            if os.path.exists(src_file):
                shutil.copy2(src_file, os.path.join(plugin_target_dir, f))
                
        views_src = os.path.join(source_dir, "Views")
        if os.path.exists(views_src):
            shutil.copytree(views_src, os.path.join(plugin_target_dir, "Views"))
            
    if os.path.exists(output_zip_path):
        os.remove(output_zip_path)
        
    os.makedirs(os.path.dirname(output_zip_path), exist_ok=True)
    
    with zipfile.ZipFile(output_zip_path, 'w', zipfile.ZIP_DEFLATED) as zipf:
        for root, dirs, files in os.walk(temp_dir):
            for file in files:
                full_path = os.path.join(root, file)
                rel_path = os.path.relpath(full_path, temp_dir)
                zipf.write(full_path, rel_path)
                
    shutil.rmtree(temp_dir)
    size_kb = os.path.getsize(output_zip_path) / 1024
    print(f"Created Combined Bundle: {output_zip_path} ({size_kb:.1f} KB)")


if __name__ == "__main__":
    base_dir = r"e:\projects\nopCommerce_4.90.3_Source"
    deploy_dir = os.path.join(base_dir, "DeployablePackages")
    plugins_build_dir = os.path.join(base_dir, "src", "Presentation", "Nop.Web", "Plugins")
    
    gp_source = os.path.join(plugins_build_dir, "Misc.GroupPurchase")
    sm_source = os.path.join(plugins_build_dir, "Misc.SellerMarketing")
    
    # 1. Package Misc.GroupPurchase
    gp_zip = os.path.join(deploy_dir, "Misc.GroupPurchase.zip")
    make_plugin_zip(gp_source, "Misc.GroupPurchase", gp_zip)
    
    # 2. Package Misc.SellerMarketing
    sm_zip = os.path.join(deploy_dir, "Misc.SellerMarketing.zip")
    make_plugin_zip(sm_source, "Misc.SellerMarketing", sm_zip)
    
    # 3. Combined package of both updated modules
    combined_zip = os.path.join(deploy_dir, "Updated_Modules_GroupPurchase_And_SellerMarketing.zip")
    make_combined_bundle([
        (gp_source, "Misc.GroupPurchase"),
        (sm_source, "Misc.SellerMarketing")
    ], combined_zip)

    # 4. Full bundle with all 6 custom modules
    all_bundle_zip = os.path.join(deploy_dir, "All_Custom_Modules_Bundle.zip")
    all_custom_plugins = [
        (gp_source, "Misc.GroupPurchase"),
        (sm_source, "Misc.SellerMarketing"),
        (os.path.join(plugins_build_dir, "Misc.AmazingDiscounts"), "Misc.AmazingDiscounts"),
        (os.path.join(plugins_build_dir, "Misc.ArtificialIntelligence"), "Misc.ArtificialIntelligence"),
        (os.path.join(plugins_build_dir, "Misc.UserNotifications"), "Misc.UserNotifications"),
        (os.path.join(plugins_build_dir, "Shipping.ConditionalMethods"), "Shipping.ConditionalMethods")
    ]
    make_combined_bundle(all_custom_plugins, all_bundle_zip)
    
    print("\nPackaging completed successfully!")
